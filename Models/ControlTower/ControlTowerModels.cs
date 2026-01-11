using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.ControlTower
{
    public sealed class AlertsConfSensors
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;

        [JsonPropertyName("lower")]
        public double? Lower { get; set; }

        [JsonPropertyName("higher")]
        public double? Higher { get; set; }

        [JsonPropertyName("ideal")]
        public double? Ideal { get; set; }

        [JsonPropertyName("products_names")]
        public string? ProductsNames { get; set; }

        [JsonPropertyName("notification_time")]
        public double? NotificationTime { get; set; }

        [JsonPropertyName("is_recurrent")]
        public bool IsRecurrent { get; set; }

        [JsonPropertyName("products_id")]
        public List<string> ProductsId { get; set; } = new();
    }

    public sealed class OrderAlertsConfSensors
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;

        [JsonPropertyName("lower")]
        public double? Lower { get; set; }

        [JsonPropertyName("higher")]
        public double? Higher { get; set; }

        [JsonPropertyName("ideal")]
        public double? Ideal { get; set; }

        [JsonPropertyName("products_id")]
        public List<string> ProductsId { get; set; } = new();

        [JsonPropertyName("products")]
        public List<Product>? Products { get; set; }

        [JsonPropertyName("notification_time")]
        public double? NotificationTime { get; set; }

        [JsonPropertyName("is_recurrent")]
        public bool IsRecurrent { get; set; }

        [JsonPropertyName("device_codes")]
        public List<string> DeviceCodes { get; set; } = new();
    }

    public sealed class ShipmentAlertsConfSensors
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;

        [JsonPropertyName("lower")]
        public double? Lower { get; set; }

        [JsonPropertyName("higher")]
        public double? Higher { get; set; }

        [JsonPropertyName("ideal")]
        public double? Ideal { get; set; }

        [JsonPropertyName("products_id")]
        public List<string> ProductsId { get; set; } = new();

        [JsonPropertyName("products")]
        public List<Product>? Products { get; set; }

        [JsonPropertyName("notification_time")]
        public double? NotificationTime { get; set; }

        [JsonPropertyName("is_recurrent")]
        public bool IsRecurrent { get; set; }

        [JsonPropertyName("device_codes")]
        public List<string> DeviceCodes { get; set; } = new();

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }
    }

    public sealed class OrderContent
    {
        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("variety")]
        public string? Variety { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("weight_unit")]
        public string? WeightUnit { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public sealed class OrderEntity
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("entity_id")]
        public string? EntityId { get; set; }
    }

    public sealed class OrderLoadUnloadPoint
    {
        [JsonPropertyName("location_id")]
        public string? LocationId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset? Date { get; set; }

        [JsonPropertyName("content")]
        public OrderContent? Content { get; set; }

        [JsonPropertyName("entity")]
        public OrderEntity? Entity { get; set; }
    }

    public sealed class OrderTracking
    {
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    public sealed class OrderGroup
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public sealed class ProductGroup
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public sealed class Product
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("variety")]
        public string? Variety { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("weight_unit")]
        public string? WeightUnit { get; set; }

        [JsonPropertyName("groups")]
        public List<ProductGroup> Groups { get; set; } = new();
    }

    public class OrderProtocolDocument
    {
        [JsonPropertyName("protocol_document_id")]
        public string? ProtocolDocumentId { get; set; }

        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        [JsonPropertyName("file_b64")]
        public string? FileBase64 { get; set; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }

        [JsonPropertyName("type_id")]
        public string? TypeId { get; set; }

        [JsonPropertyName("type_code")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("file_last_update")]
        public string? FileLastUpdate { get; set; }

        [JsonPropertyName("file_change_log")]
        public object? FileChangeLog { get; set; }
    }

    public sealed class OrderProtocolDocumentWithFile : OrderProtocolDocument
    {
        [JsonPropertyName("file_b64")]
        public new string? FileBase64 { get; set; }
    }

    public class Order
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("company_id")]
        public string? CompanyId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "INCOMPLETE";

        [JsonPropertyName("status_detail")]
        public string? StatusDetail { get; set; }

        [JsonPropertyName("shipment_status")]
        public string? ShipmentStatus { get; set; }

        [JsonPropertyName("shipment_id")]
        public string? ShipmentId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("code_internal")]
        public string? CodeInternal { get; set; }

        [JsonPropertyName("code_client_order")]
        public string? CodeClientOrder { get; set; }

        [JsonPropertyName("code_customer")]
        public string? CodeCustomer { get; set; }

        [JsonPropertyName("creation_date")]
        public DateOnly? CreationDate { get; set; }

        [JsonPropertyName("deliver_date")]
        public DateOnly? DeliverDate { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("protocol_id")]
        public string? ProtocolId { get; set; }

        [JsonPropertyName("shipment_name")]
        public string? ShipmentName { get; set; }

        [JsonPropertyName("content_names")]
        public string? ContentNames { get; set; }

        [JsonPropertyName("load_points_codes")]
        public string? LoadPointsCodes { get; set; }

        [JsonPropertyName("unload_points_codes")]
        public string? UnloadPointsCodes { get; set; }

        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("protocol_documents_ok")]
        public bool? ProtocolDocumentsOk { get; set; }

        [JsonPropertyName("protocol_documents_status")]
        public string? ProtocolDocumentsStatus { get; set; }

        [JsonPropertyName("additional_data")]
        public object? AdditionalData { get; set; }

        [JsonPropertyName("documents")]
        public List<OrderProtocolDocument>? Documents { get; set; }

        [JsonPropertyName("content")]
        public List<OrderContent>? Content { get; set; }

        [JsonPropertyName("load_points")]
        public List<OrderLoadUnloadPoint>? LoadPoints { get; set; }

        [JsonPropertyName("unload_points")]
        public List<OrderLoadUnloadPoint>? UnloadPoints { get; set; }

        [JsonPropertyName("tracking")]
        public List<OrderTracking>? Tracking { get; set; }

        [JsonPropertyName("groups")]
        public List<OrderGroup>? Groups { get; set; }

        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new();

        [JsonPropertyName("sensor_alerts_conf")]
        public List<OrderAlertsConfSensors>? SensorAlerts { get; set; }
    }

    public sealed class OrderWithFiles : Order
    {
        [JsonPropertyName("documents")]
        public new List<OrderProtocolDocumentWithFile>? Documents { get; set; }
    }

    public sealed class ShipmentLocation
    {
        [JsonPropertyName("location_id")]
        public string? LocationId { get; set; }

        [JsonPropertyName("loading_idx")]
        public int? LoadingIndex { get; set; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset? Date { get; set; }

        [JsonPropertyName("hour")]
        public string? Hour { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }

        [JsonPropertyName("content")]
        public OrderContent? Content { get; set; }

        [JsonPropertyName("entity")]
        public ShipmentEntity? Entity { get; set; }
    }

    public sealed class ShipmentEntity
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("entity_id")]
        public string? EntityId { get; set; }
    }

    public sealed class ShipmentTransporter
    {
        [JsonPropertyName("internal_id")]
        public string? InternalId { get; set; }

        [JsonPropertyName("entity_id")]
        public string? EntityId { get; set; }

        [JsonPropertyName("entity_name")]
        public string? EntityName { get; set; }

        [JsonPropertyName("license_plate")]
        public string? LicensePlate { get; set; }

        [JsonPropertyName("transport_type")]
        public string? TransportType { get; set; }

        [JsonPropertyName("etd")]
        public DateTimeOffset? Etd { get; set; }

        [JsonPropertyName("eta")]
        public DateTimeOffset? Eta { get; set; }

        [JsonPropertyName("transshipment_date")]
        public DateTimeOffset? TransshipmentDate { get; set; }

        [JsonPropertyName("origin_port")]
        public string? OriginPort { get; set; }

        [JsonPropertyName("destiny_port")]
        public string? DestinyPort { get; set; }

        [JsonPropertyName("transshipment_port")]
        public string? TransshipmentPort { get; set; }

        [JsonPropertyName("mbl")]
        public string? Mbl { get; set; }

        [JsonPropertyName("setting")]
        public string? Setting { get; set; }

        [JsonPropertyName("double_driver")]
        public bool? DoubleDriver { get; set; }

        [JsonPropertyName("change_pallets")]
        public int ChangePallets { get; set; }

        [JsonPropertyName("temperature_control")]
        public string? TemperatureControl { get; set; }

        [JsonPropertyName("consignee")]
        public string? Consignee { get; set; }

        [JsonPropertyName("ship")]
        public string? Ship { get; set; }
    }

    public class Shipment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("company_id")]
        public string? CompanyId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("company_code")]
        public string? CompanyCode { get; set; }

        [JsonPropertyName("date_deliver")]
        public DateOnly? DateDeliver { get; set; }

        [JsonPropertyName("date_deliver_actual")]
        public DateTimeOffset? DateDeliverActual { get; set; }

        [JsonPropertyName("date_emission")]
        public DateTimeOffset? DateEmission { get; set; }

        [JsonPropertyName("date_emission_actual")]
        public DateTimeOffset? DateEmissionActual { get; set; }

        [JsonPropertyName("date_documents_completion")]
        public DateTimeOffset? DateDocumentsCompletion { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDING";

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [JsonPropertyName("device_origin")]
        public string? DeviceOrigin { get; set; }

        [JsonPropertyName("shipment_buy_price")]
        public double? ShipmentBuyPrice { get; set; }

        [JsonPropertyName("shipment_sell_price")]
        public double? ShipmentSellPrice { get; set; }

        [JsonPropertyName("orders_codes")]
        public string? OrdersCodes { get; set; }

        [JsonPropertyName("load_points_codes")]
        public string? LoadPointsCodes { get; set; }

        [JsonPropertyName("unload_points_codes")]
        public string? UnloadPointsCodes { get; set; }

        [JsonPropertyName("orders_customers_codes")]
        public string? OrdersCustomersCodes { get; set; }

        [JsonPropertyName("transporters_names")]
        public string? TransportersNames { get; set; }

        [JsonPropertyName("transporter_str")]
        public string? TransporterStr { get; set; }

        [JsonPropertyName("eta_str")]
        public string? EtaStr { get; set; }

        [JsonPropertyName("etd_str")]
        public string? EtdStr { get; set; }

        [JsonPropertyName("origin_port_str")]
        public string? OriginPortStr { get; set; }

        [JsonPropertyName("destination_port_str")]
        public string? DestinationPortStr { get; set; }

        [JsonPropertyName("origin_str")]
        public string? OriginStr { get; set; }

        [JsonPropertyName("destination_str")]
        public string? DestinationStr { get; set; }

        [JsonPropertyName("ship_name")]
        public string? ShipName { get; set; }

        [JsonPropertyName("content_str")]
        public string? ContentStr { get; set; }

        [JsonPropertyName("license_plate_str")]
        public string? LicensePlateStr { get; set; }

        [JsonPropertyName("transport_type_str")]
        public string? TransportTypeStr { get; set; }

        [JsonPropertyName("customer_str")]
        public string? CustomerStr { get; set; }

        [JsonPropertyName("protocol_documents_ok")]
        public bool? ProtocolDocumentsOk { get; set; }

        [JsonPropertyName("protocol_documents_status")]
        public string? ProtocolDocumentsStatus { get; set; }

        [JsonPropertyName("marine_notification_enabled")]
        public bool MarineNotificationEnabled { get; set; }

        [JsonPropertyName("has_shipment_link")]
        public bool HasShipmentLink { get; set; }

        [JsonPropertyName("has_reports_link")]
        public bool HasReportsLink { get; set; }

        [JsonPropertyName("has_tracking_link")]
        public bool HasTrackingLink { get; set; }

        [JsonPropertyName("has_documentation_link")]
        public bool HasDocumentationLink { get; set; }

        [JsonPropertyName("public_link")]
        public string? PublicLink { get; set; }

        [JsonPropertyName("total_amount")]
        public double? TotalAmount { get; set; }

        [JsonPropertyName("eta_estimated")]
        public DateTimeOffset? EtaEstimated { get; set; }

        [JsonPropertyName("from_other_company")]
        public string? FromOtherCompany { get; set; }

        [JsonPropertyName("included_by_group_with_edition")]
        public string? IncludedByGroupWithEdition { get; set; }

        [JsonPropertyName("additional_data")]
        public object? AdditionalData { get; set; }

        [JsonPropertyName("orders")]
        public List<Order>? Orders { get; set; }

        [JsonPropertyName("new_orders")]
        public List<Order>? NewOrders { get; set; }

        [JsonPropertyName("orders_id")]
        public List<string> OrdersId { get; set; } = new();

        [JsonPropertyName("locations_load")]
        public List<ShipmentLocation>? LocationsLoad { get; set; }

        [JsonPropertyName("locations_unload")]
        public List<ShipmentLocation>? LocationsUnload { get; set; }

        [JsonPropertyName("transporters")]
        public List<ShipmentTransporter>? Transporters { get; set; }

        [JsonPropertyName("documents")]
        public List<OrderProtocolDocument>? Documents { get; set; }

        [JsonPropertyName("sensor_alerts_conf")]
        public List<ShipmentAlertsConfSensors>? SensorAlertsConf { get; set; }
    }

    public sealed class ShipmentAndOrder : Shipment
    {
        [JsonPropertyName("order_code")]
        public string? OrderCode { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("order_creation_date")]
        public DateOnly? OrderCreationDate { get; set; }

        [JsonPropertyName("order_deliver_date")]
        public DateOnly? OrderDeliverDate { get; set; }

        [JsonPropertyName("content")]
        public new List<OrderContent>? Content { get; set; }

        [JsonPropertyName("tracking")]
        public new List<OrderTracking>? Tracking { get; set; }

        [JsonPropertyName("groups")]
        public new List<OrderGroup>? Groups { get; set; }

        [JsonPropertyName("emails")]
        public new List<string> Emails { get; set; } = new();

        [JsonPropertyName("protocol_id")]
        public string? ProtocolId { get; set; }
    }

    public sealed class ShipmentLink
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("has_shipment_link")]
        public bool HasShipmentLink { get; set; }

        [JsonPropertyName("has_reports_link")]
        public bool HasReportsLink { get; set; }

        [JsonPropertyName("has_tracking_link")]
        public bool HasTrackingLink { get; set; }

        [JsonPropertyName("has_documentation_link")]
        public bool HasDocumentationLink { get; set; }

        [JsonPropertyName("public_link")]
        public string? PublicLink { get; set; }
    }

    public sealed class ClaimFile
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }

        [JsonPropertyName("file_b64")]
        public string? FileBase64 { get; set; }

        [JsonPropertyName("type_id")]
        public string TypeId { get; set; } = string.Empty;

        [JsonPropertyName("type_code")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("order_name")]
        public string? OrderName { get; set; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("shipment_name")]
        public string? ShipmentName { get; set; }

        [JsonPropertyName("shipment_id")]
        public string? ShipmentId { get; set; }

        [JsonPropertyName("file_last_update")]
        public string? FileLastUpdate { get; set; }
    }

    public sealed class ClaimProduct
    {
        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("variety")]
        public string? Variety { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("weight_unit")]
        public string? WeightUnit { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("order_name")]
        public string? OrderName { get; set; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("shipment_name")]
        public string? ShipmentName { get; set; }

        [JsonPropertyName("shipment_id")]
        public string? ShipmentId { get; set; }

        [JsonPropertyName("amount_claimed")]
        public double AmountClaimed { get; set; }

        [JsonPropertyName("amount_accepted")]
        public double AmountAccepted { get; set; }

        [JsonPropertyName("quantity_claimed")]
        public double QuantityClaimed { get; set; }
    }

    public sealed class ClaimUpdate
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public sealed class ClaimGroup
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Claim
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("shipment_id")]
        public string? ShipmentId { get; set; }

        [JsonPropertyName("shipment_name")]
        public string? ShipmentName { get; set; }

        [JsonPropertyName("internal_code")]
        public string? InternalCode { get; set; }

        [JsonPropertyName("external_code")]
        public string? ExternalCode { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("date_init")]
        public DateTimeOffset? DateInit { get; set; }

        [JsonPropertyName("date_end")]
        public DateTimeOffset? DateEnd { get; set; }

        [JsonPropertyName("date_resolution")]
        public DateTimeOffset? DateResolution { get; set; }

        [JsonPropertyName("date_last_update")]
        public DateTimeOffset? DateLastUpdate { get; set; }

        [JsonPropertyName("date_expiration")]
        public DateTimeOffset? DateExpiration { get; set; }

        [JsonPropertyName("invoice_claimed_order")]
        public string? InvoiceClaimedOrder { get; set; }

        [JsonPropertyName("invoice_claim")]
        public string? InvoiceClaim { get; set; }

        [JsonPropertyName("invoice_claim_payment")]
        public string? InvoiceClaimPayment { get; set; }

        [JsonPropertyName("amount_claimed")]
        public double? AmountClaimed { get; set; }

        [JsonPropertyName("accepted_amount")]
        public double? AcceptedAmount { get; set; }

        [JsonPropertyName("resolution")]
        public string? Resolution { get; set; }

        [JsonPropertyName("orders_names")]
        public string? OrdersNames { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("shipowner_id")]
        public string? ShipownerId { get; set; }

        [JsonPropertyName("license_plate")]
        public string? LicensePlate { get; set; }

        [JsonPropertyName("documents_status")]
        public string? DocumentsStatus { get; set; }

        [JsonPropertyName("additional_data")]
        public object? AdditionalData { get; set; }

        [JsonPropertyName("orders")]
        public List<Order>? Orders { get; set; }

        [JsonPropertyName("orders_id")]
        public List<string> OrdersId { get; set; } = new();

        [JsonPropertyName("files")]
        public List<ClaimFile>? Files { get; set; }

        [JsonPropertyName("products")]
        public List<ClaimProduct>? Products { get; set; }

        [JsonPropertyName("updates")]
        public List<ClaimUpdate>? Updates { get; set; }

        [JsonPropertyName("groups")]
        public List<ClaimGroup>? Groups { get; set; }
    }

    public sealed class ClaimId : Claim
    {
        [JsonPropertyName("orders")]
        public new List<OrderWithFiles>? Orders { get; set; }
    }

    public sealed class CompanyGroupContact
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("telephone")]
        public string? Telephone { get; set; }

        [JsonPropertyName("contact_id")]
        public string? ContactId { get; set; }

        [JsonPropertyName("with_notifications")]
        public bool WithNotifications { get; set; } = true;

        [JsonPropertyName("with_email_notifications")]
        public bool WithEmailNotifications { get; set; } = true;

        [JsonPropertyName("with_views")]
        public bool WithViews { get; set; } = true;

        [JsonPropertyName("with_edition")]
        public bool WithEdition { get; set; } = true;

        [JsonPropertyName("with_temperature_alerts")]
        public bool WithTemperatureAlerts { get; set; } = true;

        [JsonPropertyName("with_humidity_alerts")]
        public bool WithHumidityAlerts { get; set; } = true;

        [JsonPropertyName("with_intrusion_alerts")]
        public bool WithIntrusionAlerts { get; set; } = true;

        [JsonPropertyName("with_documents_completion_alert")]
        public bool WithDocumentsCompletionAlert { get; set; } = true;

        [JsonPropertyName("with_claim_expiration_alert")]
        public bool WithClaimExpirationAlert { get; set; } = true;

        [JsonPropertyName("with_automatic_final_summary")]
        public bool WithAutomaticFinalSummary { get; set; } = true;

        [JsonPropertyName("with_container_tracking_alert")]
        public bool WithContainerTrackingAlert { get; set; } = true;
    }

    public sealed class CompanyGroupElement
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public sealed class CompanyGroup
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("company_id")]
        public string? CompanyId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parent")]
        public CompanyGroupElement? Parent { get; set; }

        [JsonPropertyName("parent_name")]
        public string? ParentName { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("contacts")]
        public List<CompanyGroupContact>? Contacts { get; set; }
    }

    public sealed class Coordinates
    {
        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }
    }

    public sealed class EntityGroup
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;
    }

    public sealed class Location
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("location_id")]
        public string? LocationId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("coordinates")]
        public Coordinates? Coordinates { get; set; }

        [JsonPropertyName("groups")]
        public List<EntityGroup>? Groups { get; set; }
    }

    public sealed class Entity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public List<string> Type { get; set; } = new();

        [JsonPropertyName("transport_type")]
        public string? TransportType { get; set; }

        [JsonPropertyName("type_str")]
        public string? TypeStr { get; set; }

        [JsonPropertyName("location")]
        public List<Location>? Location { get; set; }

        [JsonPropertyName("groups")]
        public List<EntityGroup>? Groups { get; set; }
    }

    public sealed class Dictionaries
    {
        [JsonPropertyName("order_status")]
        public Dictionary<string, object> OrderStatus { get; set; } = new();

        [JsonPropertyName("shipment_status")]
        public Dictionary<string, object> ShipmentStatus { get; set; } = new();

        [JsonPropertyName("route_type")]
        public Dictionary<string, object> RouteType { get; set; } = new();

        [JsonPropertyName("countries")]
        public Dictionary<string, object> Countries { get; set; } = new();

        [JsonPropertyName("device_providers")]
        public Dictionary<string, object> DeviceProviders { get; set; } = new();

        [JsonPropertyName("tools")]
        public Dictionary<string, object> Tools { get; set; } = new();

        [JsonPropertyName("parameters_names")]
        public Dictionary<string, object> ParametersNames { get; set; } = new();

        [JsonPropertyName("company_positions")]
        public Dictionary<string, object> CompanyPositions { get; set; } = new();

        [JsonPropertyName("products_families")]
        public Dictionary<string, object> ProductsFamilies { get; set; } = new();

        [JsonPropertyName("how_many_team_workers")]
        public Dictionary<string, object> HowManyTeamWorkers { get; set; } = new();

        [JsonPropertyName("document_type")]
        public Dictionary<string, object> DocumentType { get; set; } = new();

        [JsonPropertyName("job_positions")]
        public Dictionary<string, object> JobPositions { get; set; } = new();

        [JsonPropertyName("languages")]
        public Dictionary<string, object> Languages { get; set; } = new();
    }

    public sealed class DocumentType
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public sealed class Document
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("company_id")]
        public string? CompanyId { get; set; }

        [JsonPropertyName("type_id")]
        public string? TypeId { get; set; }

        [JsonPropertyName("type_code")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateOnly? Date { get; set; }

        [JsonPropertyName("reusable")]
        public bool Reusable { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; } = string.Empty;
    }

    public sealed class DeviceRegister
    {
        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; } = string.Empty;

        [JsonPropertyName("device_provider")]
        public string DeviceProvider { get; set; } = string.Empty;

        [JsonPropertyName("device_model")]
        public string? DeviceModel { get; set; }
    }

    public sealed class EventHistoricListTable
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; } = string.Empty;

        [JsonPropertyName("device_provider")]
        public string DeviceProvider { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("event_date")]
        public DateTimeOffset? EventDate { get; set; }

        [JsonPropertyName("insert_date")]
        public DateTimeOffset? InsertDate { get; set; }

        [JsonPropertyName("status")]
        public bool? Status { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; } = "0.0000000000";

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; } = "0.0000000000";
    }

    public sealed class EnumValue
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public sealed class DocumentsProtocolRule
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type_id")]
        public string TypeId { get; set; } = string.Empty;

        [JsonPropertyName("type_code")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("public")]
        public bool Public { get; set; }
    }

    public sealed class DocumentsProtocol
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("before_finish_days")]
        public int BeforeFinishDays { get; set; }

        [JsonPropertyName("after_finish_days")]
        public int AfterFinishDays { get; set; }

        [JsonPropertyName("has_before_finish_days")]
        public bool HasBeforeFinishDays { get; set; }

        [JsonPropertyName("has_after_finish_days")]
        public bool HasAfterFinishDays { get; set; }

        [JsonPropertyName("documents")]
        public List<DocumentsProtocolRule> Documents { get; set; } = new();
    }
}