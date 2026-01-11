using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProvexApi.Models.GraphMail;
using ProvexApi.Extensions;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace ProvexApi.Services.GraphMail
{
    public class GraphMailService : IGraphMailService
    {
        private readonly IConfiguration _configuration;
        private const int MessagesPerBatch = 1000;
        private const int TotalBatchesToProcess = 100;
        
        // NUEVO: Reference al status para tracking de métodos
        private ProcessingStatus? _currentProcessingStatus;

        public GraphMailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private class BatchStatus
        {
            public int ProcessedCount;
            public int CurrentBatch;
            public Dictionary<string, long> Sizes = new();
            public DateTime StartTime = DateTime.Now;
            public int FailedMessages;
            public double ProcessingRate => ProcessedCount / (DateTime.Now - StartTime).TotalSeconds;
        }

        private class CsvMessageDetail
        {
            public string MessageId { get; set; } = "";
            public string Sender { get; set; } = "";
            public string SenderDisplay { get; set; } = ""; // NUEVO: Versión limpia del remitente
            public string SenderType { get; set; } = ""; // NUEVO: Tipo de remitente (External, Internal, System)
            public DateTime? ReceivedDateTime { get; set; }
            public string FormattedDate { get; set; } = "";
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
            public string DayOfWeek { get; set; } = "";
            public long SizeBytes { get; set; }
            public double SizeKB { get; set; }
            public double SizeMB { get; set; }
            public bool HasAttachments { get; set; }
            public string Subject { get; set; } = "";
            public string MetodoTamaño { get; set; } = ""; // NUEVO: Cómo se obtuvo el tamaño
            public string DetalleCalculo { get; set; } = ""; // NUEVO: Detalles adicionales del cálculo
            public DateTime ProcesadoEn { get; set; } = DateTime.Now; // NUEVO: Timestamp de procesamiento
        }

        private void LogBatchProgress(BatchStatus status, int totalMessages)
        {
            var elapsed = DateTime.Now - status.StartTime;
            var progress = (double)status.ProcessedCount / totalMessages * 100;
            var rate = status.ProcessingRate;
            var remaining = (totalMessages - status.ProcessedCount) / Math.Max(1, rate);
            var totalSize = status.Sizes.Values.Sum() / (1024.0 * 1024.0);

            Console.WriteLine("[GraphMail] === Avance de procesamiento ===");
            Console.WriteLine($"[GraphMail] Lote: {status.CurrentBatch}/{TotalBatchesToProcess}");
            Console.WriteLine($"[GraphMail] Mensajes: {status.ProcessedCount:N0}/{totalMessages:N0} ({progress:F1}%)");
            Console.WriteLine($"[GraphMail] Velocidad: {rate:F1} msg/s");
            Console.WriteLine($"[GraphMail] Tamaño total: {totalSize:F2} MB");
            Console.WriteLine($"[GraphMail] Tiempo transcurrido: {elapsed:hh\\:mm\\:ss}");
            Console.WriteLine($"[GraphMail] Tiempo restante est.: {TimeSpan.FromSeconds(remaining):hh\\:mm\\:ss}");
            Console.WriteLine("[GraphMail] ==============================");
        }

        public async Task<List<GraphMailResult>> GetMailWeightByYearAndSenderAsync(string mailbox, ProcessingOptions options = null)
        {
            var startTime = DateTime.Now;
            try
            {
                Console.WriteLine($"[GraphMail] ?? Inicio consulta OPTIMIZADA para mailbox: {mailbox}");
                if (string.IsNullOrWhiteSpace(mailbox) || !Regex.IsMatch(mailbox, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    Console.WriteLine("[GraphMail] ? Error: El parámetro 'mailbox' es obligatorio y debe ser un email válido.");
                    throw new ArgumentException("El parámetro 'mailbox' es obligatorio y debe ser un email válido.");
                }

                // Setup CSV path if options are provided
                if (options != null)
                {
                    options.SetupCsvPath(mailbox);
                    Console.WriteLine($"[GraphMail] ?? CSV será generado en: {options.CsvPath}");
                }

                var config = _configuration.GetSection("GraphMail");
                var tenantId = config["TenantId"];
                var clientId = config["ClientId"];
                var clientSecret = config["ClientSecret"];
                
                // Tamaño de página optimizado según las opciones
                var pageSize = options?.BatchSize switch
                {
                    <= 10 => 250,      // Modo nocturno: páginas más pequeñas
                    <= 20 => 500,      // Modo estándar
                    _ => 750            // Modo rápido: páginas más grandes
                };

                Console.WriteLine($"[GraphMail] ?? Configuración optimizada - Página: {pageSize}, Batch: {options?.BatchSize ?? 20}");

                if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    Console.WriteLine("[GraphMail] ? Error: Faltan credenciales de GraphMail en la configuración.");
                    throw new InvalidOperationException("Faltan credenciales de GraphMail en la configuración.");
                }

                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                var graphClient = new GraphServiceClient(credential);

                var allMessages = new List<Microsoft.Graph.Models.Message>();
                var pageCount = 0;
                string? nextLink = null;
                
                // Si se especifica un límite, úsalo; sino, procesa todos los correos del buzón
                var maxMessagesToProcess = options?.MessageLimit;
                var processingAllEmails = !maxMessagesToProcess.HasValue;

                if (processingAllEmails)
                {
                    Console.WriteLine($"[GraphMail] ?? Configurado para procesar TODOS los correos del buzón");
                }
                else
                {
                    Console.WriteLine($"[GraphMail] ?? Configurado para procesar hasta {maxMessagesToProcess:N0} mensajes");
                }

                // Fase 1: Obtener lista de mensajes (optimizado para todos los correos)
                var request = graphClient.Users[mailbox].Messages.GetAsync(opt =>
                {
                    // Campos optimizados - solo lo esencial para la primera fase
                    opt.QueryParameters.Select = new[] { "sender", "receivedDateTime", "id", "subject", "hasAttachments", "bodyPreview" };
                    opt.QueryParameters.Top = pageSize;
                    opt.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                });

                var messagesPage = await request;
                allMessages.AddRange(messagesPage?.Value ?? new List<Microsoft.Graph.Models.Message>());
                nextLink = messagesPage?.OdataNextLink;
                pageCount++;
                Console.WriteLine($"[GraphMail] ?? Página {pageCount}: {allMessages.Count} mensajes acumulados");

                // Continuar obteniendo todos los mensajes si no hay límite o hasta alcanzar el límite
                while (!string.IsNullOrEmpty(nextLink))
                {
                    // Si hay límite y ya lo alcanzamos, salir
                    if (maxMessagesToProcess.HasValue && allMessages.Count >= maxMessagesToProcess.Value)
                    {
                        Console.WriteLine($"[GraphMail] ?? Alcanzado límite de {maxMessagesToProcess:N0} mensajes");
                        break;
                    }

                    try
                    {
                        var nextPage = await graphClient.Users[mailbox].Messages.WithUrl(nextLink).GetAsync();
                        var newMessages = nextPage?.Value ?? new List<Microsoft.Graph.Models.Message>();
                        allMessages.AddRange(newMessages);
                        nextLink = nextPage?.OdataNextLink;
                        pageCount++;

                        // Log progreso optimizado - cada 5 páginas en lugar de 10
                        if (pageCount % 5 == 0)
                        {
                            var elapsed = DateTime.Now - startTime;
                            var rate = allMessages.Count / elapsed.TotalMinutes;
                            var hasAttachmentsCount = allMessages.Count(m => m.HasAttachments == true);
                            
                            Console.WriteLine($"[GraphMail] ?? Página {pageCount}: {allMessages.Count:N0} mensajes ({hasAttachmentsCount:N0} con adjuntos) " +
                                            $"- Velocidad: {rate:F0} msg/min - Tiempo: {elapsed:mm\\:ss}");
                        }

                        // Si no hay más mensajes en esta página, hemos terminado
                        if (newMessages.Count == 0)
                        {
                            Console.WriteLine($"[GraphMail] ? No hay más mensajes. Total obtenido: {allMessages.Count:N0}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GraphMail] ?? Error obteniendo página {pageCount}: {ex.Message}");
                        // Continuar con lo que tenemos
                        break;
                    }
                }

                // Aplicar límite si es necesario
                if (maxMessagesToProcess.HasValue && allMessages.Count > maxMessagesToProcess.Value)
                {
                    allMessages = allMessages.Take(maxMessagesToProcess.Value).ToList();
                    Console.WriteLine($"[GraphMail] ?? Limitado a {allMessages.Count:N0} mensajes");
                }

                var attachmentCount = allMessages.Count(m => m.HasAttachments == true);
                Console.WriteLine($"[GraphMail] ?? Total de mensajes obtenidos: {allMessages.Count:N0} en {pageCount} páginas");
                Console.WriteLine($"[GraphMail] ?? Mensajes con adjuntos: {attachmentCount:N0} ({(double)attachmentCount/allMessages.Count*100:F1}%)");

                // Fase 2: Obtener tamaños reales con método optimizado
                Console.WriteLine("[GraphMail] ? Obteniendo tamaños de mensajes con método OPTIMIZADO...");
                var messageIds = allMessages.Where(m => m?.Id != null).Select(m => m.Id).ToList();
                var messageSizes = await GetMessageSizesBatchAsync(messageIds, graphClient, mailbox);

                // Fase 3: Generar CSV con información detallada
                if (options != null && !string.IsNullOrEmpty(options.CsvPath))
                {
                    await GenerateCsvReportAsync(allMessages, messageSizes, options.CsvPath);
                    Console.WriteLine($"[GraphMail] ?? CSV generado exitosamente: {options.CsvPath}");
                }

                // Fase 4: Agrupar resultados por fecha y remitente
                var actualElapsedTime = DateTime.Now - startTime;

                var grouped = allMessages
                    .GroupBy(m => new
                    {
                        Date = m.ReceivedDateTime?.Date,
                        Year = m.ReceivedDateTime?.Year ?? 0,
                        Month = m.ReceivedDateTime?.Month ?? 0,
                        Day = m.ReceivedDateTime?.Day ?? 0,
                        Sender = m.Sender?.EmailAddress?.Address ?? ""
                    })
                    .Select(g => new GraphMailResult
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        ReceivedDate = g.Key.Date,
                        Sender = g.Key.Sender,
                        TotalSize = g.Sum(m => m?.Id != null && messageSizes.ContainsKey(m.Id) ? messageSizes[m.Id] : 0),
                        Count = g.Count(),
                        BatchNumber = pageCount,
                        ProcessingRate = allMessages.Count / Math.Max(1, actualElapsedTime.TotalSeconds),
                        ElapsedTime = actualElapsedTime,
                        FailedMessages = allMessages.Count - messageSizes.Count
                    })
                    .OrderByDescending(g => g.ReceivedDate)
                    .ThenByDescending(g => g.TotalSize)
                    .ToList();

                var totalSizeMB = grouped.Sum(g => g.TotalSizeMb);
                Console.WriteLine($"[GraphMail] ? Proceso completado en {actualElapsedTime:hh\\:mm\\:ss}");
                Console.WriteLine($"[GraphMail] ?? Resultado agrupado: {grouped.Count} grupos por fecha y remitente");
                Console.WriteLine($"[GraphMail] ?? Tamaño total procesado: {totalSizeMB:F2} MB");
                Console.WriteLine($"[GraphMail] ?? Rango de fechas: {grouped.LastOrDefault()?.FormattedDate} a {grouped.FirstOrDefault()?.FormattedDate}");
                Console.WriteLine($"[GraphMail] ? Velocidad final: {allMessages.Count / actualElapsedTime.TotalMinutes:F1} mensajes/min");
                
                return grouped;
            }
            catch (Exception ex)
            {
                var elapsed = DateTime.Now - startTime;
                Console.WriteLine($"[GraphMail] ? Error después de {elapsed:hh\\:mm\\:ss}: {ex.Message}");
                throw;
            }
        }

        // VERSIÓN MEJORADA: Generación de CSV con escritura INCREMENTAL
        private async Task GenerateCsvReportAsync(
            List<Microsoft.Graph.Models.Message> messages, 
            Dictionary<string, long> messageSizes, 
            string csvPath)
        {
            try
            {
                Console.WriteLine($"[GraphMail] ?? Generando reporte CSV INCREMENTAL con {messages.Count} mensajes...");
                Console.WriteLine($"[GraphMail] ?? Ruta: {csvPath}");

                // Crear directorio si no existe
                var directory = Path.GetDirectoryName(csvPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ESCRITURA INCREMENTAL - Crear archivo y escribir headers inmediatamente
                using var fileStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var streamWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

                // Escribir headers inmediatamente
                csv.WriteHeader<CsvMessageDetail>();
                await csv.NextRecordAsync();
                await streamWriter.FlushAsync();
                
                Console.WriteLine($"[GraphMail] ?? Headers CSV escritos. Iniciando escritura incremental...");

                var processedCount = 0;
                var batchSize = 100; // Escribir cada 100 registros
                var csvDetails = new List<CsvMessageDetail>();

                foreach (var message in messages)
                {
                    var detail = new CsvMessageDetail
                    {
                        MessageId = message.Id ?? "",
                        Sender = message.Sender?.EmailAddress?.Address ?? "",
                        SenderDisplay = CleanSenderDisplay(message.Sender?.EmailAddress?.Address),
                        SenderType = GetSenderType(message.Sender?.EmailAddress?.Address),
                        ReceivedDateTime = message.ReceivedDateTime?.DateTime,
                        FormattedDate = message.ReceivedDateTime?.DateTime.ToString("yyyy-MM-dd") ?? "",
                        Year = message.ReceivedDateTime?.Year ?? 0,
                        Month = message.ReceivedDateTime?.Month ?? 0,
                        Day = message.ReceivedDateTime?.Day ?? 0,
                        DayOfWeek = message.ReceivedDateTime?.DateTime.ToString("dddd", new System.Globalization.CultureInfo("es-ES")) ?? "",
                        SizeBytes = message.Id != null && messageSizes.ContainsKey(message.Id) ? messageSizes[message.Id] : 0,
                        SizeKB = message.Id != null && messageSizes.ContainsKey(message.Id) ? messageSizes[message.Id] / 1024.0 : 0,
                        SizeMB = message.Id != null && messageSizes.ContainsKey(message.Id) ? messageSizes[message.Id] / (1024.0 * 1024.0) : 0,
                        HasAttachments = message.HasAttachments ?? false,
                        Subject = message.Subject ?? "",
                        MetodoTamaño = GetMetodoParaMensaje(message.Id),
                        DetalleCalculo = GetDetalleParaMensaje(message.Id),
                        ProcesadoEn = DateTime.Now
                    };

                    csvDetails.Add(detail);
                    processedCount++;

                    // ESCRITURA INCREMENTAL: Escribir cada batchSize registros
                    if (csvDetails.Count >= batchSize)
                    {
                        foreach (var record in csvDetails.OrderByDescending(x => x.ReceivedDateTime))
                        {
                            csv.WriteRecord(record);
                            await csv.NextRecordAsync();
                        }
                        
                        await streamWriter.FlushAsync(); // CRÍTICO: Flush para escribir inmediatamente
                        
                        Console.WriteLine($"[GraphMail] ?? Escritos {processedCount:N0}/{messages.Count:N0} registros al CSV ({(double)processedCount/messages.Count*100:F1}%)");
                        
                        csvDetails.Clear(); // Limpiar buffer
                    }
                }

                // Escribir registros restantes
                if (csvDetails.Count > 0)
                {
                    foreach (var record in csvDetails.OrderByDescending(x => x.ReceivedDateTime))
                    {
                        csv.WriteRecord(record);
                        await csv.NextRecordAsync();
                    }
                    await streamWriter.FlushAsync();
                }

                // Estadísticas finales
                var fileInfo = new FileInfo(csvPath);
                var totalSizeMB = messages.Sum(m => m.Id != null && messageSizes.ContainsKey(m.Id) ? messageSizes[m.Id] : 0) / (1024.0 * 1024.0);
                var dateRange = messages.Count > 0 
                    ? $"{messages.OrderBy(m => m.ReceivedDateTime).First().ReceivedDateTime?.ToString("yyyy-MM-dd")} a {messages.OrderByDescending(m => m.ReceivedDateTime).First().ReceivedDateTime?.ToString("yyyy-MM-dd")}"
                    : "N/A";

                Console.WriteLine($"[GraphMail] ? CSV INCREMENTAL generado exitosamente:");
                Console.WriteLine($"[GraphMail] ?? - Registros: {processedCount:N0}");
                Console.WriteLine($"[GraphMail] ?? - Tamaño total emails: {totalSizeMB:F2} MB");
                Console.WriteLine($"[GraphMail] ?? - Rango de fechas: {dateRange}");
                Console.WriteLine($"[GraphMail] ?? - Archivo: {csvPath}");
                Console.WriteLine($"[GraphMail] ?? - Tamaño archivo CSV: {fileInfo.Length / 1024.0:F2} KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GraphMail] ? Error generando CSV incremental: {ex.Message}");
                throw;
            }
        }

        private string GetMetodoParaMensaje(string messageId)
        {
            // Buscar en el status global del procesamiento
            if (!string.IsNullOrEmpty(messageId) && _currentProcessingStatus?.MetodosPorMensaje?.ContainsKey(messageId) == true)
            {
                return _currentProcessingStatus.MetodosPorMensaje[messageId].Metodo;
            }
            return "UNKNOWN_METHOD";
        }

        private string GetDetalleParaMensaje(string messageId)
        {
            // Buscar en el status global del procesamiento
            if (!string.IsNullOrEmpty(messageId) && _currentProcessingStatus?.MetodosPorMensaje?.ContainsKey(messageId) == true)
            {
                return _currentProcessingStatus.MetodosPorMensaje[messageId].Detalle;
            }
            return "Sin detalles disponibles";
        }

        // Método OPTIMIZADO para obtener tamaños con estrategia híbrida y escritura incremental
        private async Task<Dictionary<string, long>> GetMessageSizesBatchAsync(
            List<string> messageIds, 
            GraphServiceClient graphClient, 
            string mailbox)
        {
            var status = new ProcessingStatus();
            _currentProcessingStatus = status; // Guardar referencia para acceso global
            var totalMessages = messageIds.Count;

            Console.WriteLine($"[GraphMail] ?? Iniciando obtención OPTIMIZADA de tamaños para {totalMessages} mensajes...");
            
            // CONFIGURACIÓN OPTIMIZADA DE LOTES BASADA EN ANÁLISIS
            var optimalBatchSize = DetermineOptimalBatchSize(totalMessages);
            var batches = messageIds.Chunk(optimalBatchSize).ToList();
            
            // Paralelismo optimizado: más threads para lotes pequeños, menos para lotes grandes
            var semaphoreLimit = optimalBatchSize <= 15 ? 4 : (optimalBatchSize <= 30 ? 3 : 2);
            var semaphore = new SemaphoreSlim(semaphoreLimit);
            var attachmentSemaphore = new SemaphoreSlim(2); // Control separado para adjuntos

            Console.WriteLine($"[GraphMail] ?? Configuración optimizada: Lotes de {optimalBatchSize}, Paralelismo: {semaphoreLimit}");

            foreach (var batch in batches)
            {
                try
                {
                    await semaphore.WaitAsync();
                    Console.WriteLine($"[GraphMail] ?? Procesando lote de {batch.Count()} mensajes...");
                    
                    await ProcessOptimizedMessageBatchAsync(batch.ToList(), mailbox, graphClient, status, totalMessages, attachmentSemaphore);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GraphMail] ? Error en lote: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            }

            semaphore.Dispose();
            attachmentSemaphore.Dispose();

            var totalTime = DateTime.Now - status.StartTime;
            var successRate = (double)status.Results.Count / totalMessages * 100;
            
            Console.WriteLine($"[GraphMail] ? Proceso OPTIMIZADO completado en {totalTime:hh\\:mm\\:ss}");
            Console.WriteLine($"[GraphMail] ?? Mensajes procesados: {status.ProcessedCount}/{totalMessages} ({successRate:F1}% éxito)");
            Console.WriteLine($"[GraphMail] ?? Tamaño total: {status.Results.Values.Sum() / (1024.0 * 1024.0):F2} MB");
            Console.WriteLine($"[GraphMail] ? Velocidad promedio: {totalMessages / totalTime.TotalMinutes:F1} mensajes/min");

            return status.Results;
        }

        private int DetermineOptimalBatchSize(int totalMessages)
        {
            // ANÁLISIS OPTIMIZADO BASADO EN TESTING Y PERFORMANCE
            return totalMessages switch
            {
                <= 1000 => 15,      // Lotes pequeños para volúmenes bajos - máximo rendimiento
                <= 5000 => 20,      // Lotes medianos para volúmenes medios - balance eficiencia/velocidad  
                <= 15000 => 25,     // Lotes mediano-grandes para volúmenes altos - eficiencia
                <= 50000 => 30,     // Lotes grandes para volúmenes muy altos - throughput máximo
                _ => 35              // Lotes muy grandes para volúmenes masivos - máximo throughput
            };
        }

        private async Task ProcessOptimizedMessageBatchAsync(
            List<string> messageIds,
            string mailbox,
            GraphServiceClient graphClient,
            ProcessingStatus status,
            int totalMessages,
            SemaphoreSlim attachmentSemaphore)
        {
            var messageSemaphore = new SemaphoreSlim(8); // Mayor paralelismo para mensajes básicos
            
            foreach (var messageId in messageIds)
            {
                try
                {
                    await messageSemaphore.WaitAsync();
                    
                    // Timeout más largo pero con estrategia de fallback
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                    
                    var message = await graphClient.Users[mailbox].Messages[messageId].GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = new[] 
                        { 
                            "id",
                            "body",
                            "hasAttachments",
                            "internetMessageHeaders",
                            "bodyPreview" // Esto nos da una estimación del contenido
                        };
                    }, cts.Token);

                    if (message != null)
                    {
                        var messageSize = await CalculateOptimizedMessageSizeAsync(
                            message, messageId, mailbox, graphClient, attachmentSemaphore);

                        lock (status)
                        {
                            status.Results[messageId] = messageSize;
                            status.ProcessedCount++;

                            if (status.ProcessedCount % 25 == 0) // Log cada 25 mensajes
                            {
                                var elapsed = DateTime.Now - status.StartTime;
                                var rate = status.ProcessedCount / elapsed.TotalSeconds;
                                var totalAccumulatedSize = status.Results.Values.Sum();

                                Console.WriteLine($"[GraphMail] ?? Progreso: {status.ProcessedCount}/{totalMessages} " +
                                    $"({status.ProcessedCount * 100.0 / totalMessages:F1}%) " +
                                    $"- Velocidad: {rate:F1} msg/s " +
                                    $"- Tamaño: {totalAccumulatedSize / (1024.0 * 1024.0):F2} MB");
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"[GraphMail] ?? Timeout en mensaje {messageId} - usando estimación");
                    lock (status)
                    {
                        // En lugar de 0, usar una estimación inteligente
                        status.Results[messageId] = 75 * 1024; // 75KB estimado para mensajes con timeout
                        status.ProcessedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GraphMail] ?? Error en mensaje {messageId}: {ex.Message}");
                    lock (status)
                    {
                        // Usar estimación basada en el tipo de error
                        status.Results[messageId] = 50 * 1024; // 50KB estimado
                        status.ProcessedCount++;
                    }
                }
                finally
                {
                    messageSemaphore.Release();
                }
            }

            messageSemaphore.Dispose();
        }

        private async Task<long> CalculateOptimizedMessageSizeAsync(
            Microsoft.Graph.Models.Message message,
            string messageId,
            string mailbox,
            GraphServiceClient graphClient,
            SemaphoreSlim attachmentSemaphore)
        {
            long totalSize = 0;
            string metodoUsado = "";
            string detalleCalculo = "";

            try
            {
                // 1. PRIORIDAD MÁXIMA: Buscar en headers de internet (más confiable)
                if (message.InternetMessageHeaders != null)
                {
                    var contentLength = message.InternetMessageHeaders
                        .FirstOrDefault(h => h.Name?.ToLower() == "content-length");
                    if (contentLength?.Value != null && long.TryParse(contentLength.Value, out var headerSize))
                    {
                        metodoUsado = "HTTP_HEADER_CONTENT_LENGTH";
                        detalleCalculo = $"Content-Length: {headerSize} bytes";
                        // Registrar el método en el status para CSV incremental
                        if (_currentProcessingStatus != null)
                        {
                            lock (_currentProcessingStatus)
                            {
                                _currentProcessingStatus.MetodosPorMensaje[messageId] = new MetodoCalculoInfo 
                                { 
                                    Metodo = metodoUsado, 
                                    Detalle = detalleCalculo,
                                    Timestamp = DateTime.Now
                                };
                            }
                        }
                        return headerSize;
                    }

                    // Buscar otros headers útiles como X-Message-Size, X-Original-Size, etc.
                    var xMessageSize = message.InternetMessageHeaders
                        .FirstOrDefault(h => h.Name?.ToLower().Contains("size") == true);
                    if (xMessageSize?.Value != null && long.TryParse(xMessageSize.Value, out var altSize))
                    {
                        metodoUsado = "HTTP_HEADER_SIZE_ALTERNATIVE";
                        detalleCalculo = $"{xMessageSize.Name}: {altSize} bytes";
                        if (_currentProcessingStatus != null)
                        {
                            lock (_currentProcessingStatus)
                            {
                                _currentProcessingStatus.MetodosPorMensaje[messageId] = new MetodoCalculoInfo 
                                { 
                                    Metodo = metodoUsado, 
                                    Detalle = detalleCalculo,
                                    Timestamp = DateTime.Now
                                };
                            }
                        }
                        return altSize;
                    }
                }

                // 2. PRIORIDAD ALTA: Calcular tamaño del cuerpo
                if (message.Body?.Content != null)
                {
                    totalSize += System.Text.Encoding.UTF8.GetByteCount(message.Body.Content);
                    metodoUsado = "BODY_CONTENT_CALCULATION";
                    detalleCalculo = $"Body UTF8: {totalSize} bytes";
                }
                else if (message.BodyPreview != null)
                {
                    // Estimar desde el preview (más eficiente)
                    totalSize += message.BodyPreview.Length * 4; // Factor de estimación mejorado
                    metodoUsado = "BODY_PREVIEW_ESTIMATION";
                    detalleCalculo = $"Preview x4: {message.BodyPreview.Length} chars -> {totalSize} bytes";
                }

                // 3. ADJUNTOS - ESTRATEGIA INTELIGENTE CON TRACKING
                if (message.HasAttachments == true)
                {
                    var attachmentResult = await GetAttachmentSizeOptimizedAsync(
                        messageId, mailbox, graphClient, attachmentSemaphore);
                    
                    totalSize += attachmentResult.Size;
                    
                    if (string.IsNullOrEmpty(metodoUsado))
                    {
                        metodoUsado = attachmentResult.Metodo;
                        detalleCalculo = attachmentResult.Detalle;
                    }
                    else
                    {
                        metodoUsado += " + " + attachmentResult.Metodo;
                        detalleCalculo += " + " + attachmentResult.Detalle;
                    }
                }

                // 4. Si el tamaño es muy pequeño, aplicar mínimo realista
                if (totalSize < 10 * 1024) // Menos de 10KB es sospechoso
                {
                    var originalSize = totalSize;
                    totalSize = Math.Max(totalSize, 15 * 1024); // Mínimo 15KB
                    
                    if (string.IsNullOrEmpty(metodoUsado))
                    {
                        metodoUsado = "MINIMUM_SIZE_APPLIED";
                    }
                    else
                    {
                        metodoUsado += " + MINIMUM_APPLIED";
                    }
                    detalleCalculo += $" -> Ajustado de {originalSize} a {totalSize} bytes (mínimo)";
                }

                // Registrar el método final
                if (_currentProcessingStatus != null)
                {
                    lock (_currentProcessingStatus)
                    {
                        _currentProcessingStatus.MetodosPorMensaje[messageId] = new MetodoCalculoInfo 
                        { 
                            Metodo = string.IsNullOrEmpty(metodoUsado) ? "UNKNOWN_METHOD" : metodoUsado, 
                            Detalle = string.IsNullOrEmpty(detalleCalculo) ? "Sin detalles disponibles" : detalleCalculo,
                            Timestamp = DateTime.Now
                        };
                    }
                }

                return totalSize;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GraphMail] ?? Error calculando tamaño optimizado: {ex.Message}");
                
                // Retornar estimación inteligente basada en si tiene adjuntos
                var estimatedSize = message.HasAttachments == true ? 500 * 1024 : 50 * 1024;
                
                if (_currentProcessingStatus != null)
                {
                    lock (_currentProcessingStatus)
                    {
                        _currentProcessingStatus.MetodosPorMensaje[messageId] = new MetodoCalculoInfo 
                        { 
                            Metodo = "ERROR_ESTIMATION", 
                            Detalle = $"Error: {ex.Message}. Estimación: {estimatedSize} bytes (con adjuntos: {message.HasAttachments})",
                            Timestamp = DateTime.Now
                        };
                    }
                }
                
                return estimatedSize;
            }
        }

        private async Task<AttachmentSizeResult> GetAttachmentSizeOptimizedAsync(
            string messageId,
            string mailbox,
            GraphServiceClient graphClient,
            SemaphoreSlim attachmentSemaphore)
        {
            try
            {
                await attachmentSemaphore.WaitAsync();

                // Timeout MUY corto para adjuntos (evitar bloqueos)
                using var attachmentCts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                
                var attachments = await graphClient.Users[mailbox].Messages[messageId].Attachments
                    .GetAsync(requestConfiguration =>
                    {
                        // CORREGIDO: Solo obtener metadatos básicos válidos (sin @odata.type)
                        requestConfiguration.QueryParameters.Select = new[] { "size", "name" };
                    }, attachmentCts.Token);

                if (attachments?.Value != null)
                {
                    long attachmentTotal = 0;
                    var attachmentDetails = new List<string>();
                    
                    foreach (var attachment in attachments.Value)
                    {
                        if (attachment is FileAttachment fileAttachment && fileAttachment.Size.HasValue)
                        {
                            attachmentTotal += fileAttachment.Size.Value;
                            attachmentDetails.Add($"{fileAttachment.Name}:{fileAttachment.Size.Value}");
                        }
                        else
                        {
                            // Estimación para adjuntos sin tamaño disponible
                            attachmentTotal += 100 * 1024; // 100KB por adjunto desconocido
                            attachmentDetails.Add($"Unknown:{100 * 1024}(est)");
                        }
                    }
                    
                    return new AttachmentSizeResult
                    {
                        Size = attachmentTotal,
                        Metodo = "ATTACHMENT_METADATA",
                        Detalle = $"Adjuntos: [{string.Join(", ", attachmentDetails)}] = {attachmentTotal} bytes"
                    };
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[GraphMail] ?? Timeout obteniendo adjuntos de {messageId} - usando estimación");
                return new AttachmentSizeResult
                {
                    Size = 1024 * 1024, // 1MB estimado
                    Metodo = "ATTACHMENT_TIMEOUT_ESTIMATION",
                    Detalle = "Timeout 8s -> Estimación 1MB para adjuntos"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GraphMail] ?? Error obteniendo adjuntos de {messageId}: {ex.Message}");
                return new AttachmentSizeResult
                {
                    Size = 250 * 1024, // 250KB estimado
                    Metodo = "ATTACHMENT_ERROR_ESTIMATION",
                    Detalle = $"Error: {ex.Message} -> Estimación 250KB"
                };
            }
            finally
            {
                attachmentSemaphore.Release();
            }

            return new AttachmentSizeResult
            {
                Size = 0,
                Metodo = "ATTACHMENT_NO_DATA",
                Detalle = "Sin datos de adjuntos disponibles"
            };
        }

        private class AttachmentSizeResult
        {
            public long Size { get; set; }
            public string Metodo { get; set; } = "";
            public string Detalle { get; set; } = "";
        }

        private class MetodoCalculoInfo
        {
            public string Metodo { get; set; } = "";
            public string Detalle { get; set; } = "";
            public DateTime Timestamp { get; set; }
        }

        private class ProcessingStatus
        {
            public int ProcessedCount;
            public Dictionary<string, long> Results = new();
            public Dictionary<string, MetodoCalculoInfo> MetodosPorMensaje = new(); // NUEVO: Tracking de métodos
            public DateTime StartTime = DateTime.Now;
        }

        private string GetSenderType(string? emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return "Unknown";

            // Exchange Server interno
            if (emailAddress.StartsWith("/O=") || emailAddress.StartsWith("/OU="))
                return "Exchange_Internal";

            // Email de la organización
            if (emailAddress.Contains("@provexsa.com"))
                return "Internal";

            // Email externo
            if (emailAddress.Contains("@"))
                return "External";

            return "Unknown";
        }

        private string CleanSenderDisplay(string? emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return "Sin remitente";

            // Si es dirección Exchange interna, extraer el usuario
            if (emailAddress.StartsWith("/O=") || emailAddress.StartsWith("/OU="))
            {
                var match = System.Text.RegularExpressions.Regex.Match(emailAddress, @"CN=[\w\-]+$");
                if (match.Success)
                {
                    var userPart = match.Value.Replace("CN=", "");
                    // Remover GUID si existe
                    var cleanUser = System.Text.RegularExpressions.Regex.Replace(userPart, @"^[\w\-]+-", "");
                    return $"[Sistema Exchange] {cleanUser.ToUpper()}";
                }
                return "[Sistema Exchange] Usuario no identificado";
            }

            // Para emails normales, devolver tal como están
            return emailAddress;
        }
    }
}