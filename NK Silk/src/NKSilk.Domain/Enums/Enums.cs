namespace NKSilk.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Packed = 2,
    Shipped = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}

public enum PaymentMethod
{
    CashOnDelivery = 0,
    Razorpay = 1,
    PhonePe = 2,
    Upi = 3,
    CreditCard = 4,
    DebitCard = 5,
    NetBanking = 6
}

public enum ReturnStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    PickedUp = 3,
    Refunded = 4
}

public enum ReturnReason
{
    DefectiveOrDamaged = 0,
    WrongItemDelivered = 1,
    SizeOrFitIssue = 2,
    NotAsDescribed = 3,
    QualityNotSatisfactory = 4,
    ChangedMind = 5,
    Other = 6
}

public enum NotificationType
{
    General = 0,
    OrderPlaced = 1,
    OrderStatusChanged = 2,
    PaymentReceived = 3,
    ReturnRequested = 4,
    ReturnUpdate = 5,
    Shipment = 6,
    SupportReply = 7
}

public enum ShipmentStatus
{
    LabelCreated = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5
}

public enum TicketStatus
{
    Open = 0,
    AwaitingCustomer = 1,
    Resolved = 2,
    Closed = 3
}

public enum TicketCategory
{
    Order = 0,
    Payment = 1,
    ReturnRefund = 2,
    Product = 3,
    Other = 4
}

public enum OfferType
{
    PercentageOff = 0,
    FlatOff = 1
}

public enum OfferScope
{
    EntireStore = 0,
    Category = 1,
    Product = 2
}

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}

public enum DiscountType
{
    Percentage = 0,
    FlatAmount = 1
}

public enum AddressType
{
    Home = 0,
    Work = 1,
    Other = 2
}
