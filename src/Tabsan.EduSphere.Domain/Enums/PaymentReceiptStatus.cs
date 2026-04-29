namespace Tabsan.EduSphere.Domain.Enums;

/// <summary>
/// Represents the payment status of a student fee receipt.
/// </summary>
public enum PaymentReceiptStatus
{
    /// <summary>Receipt created; awaiting student payment.</summary>
    Pending = 1,

    /// <summary>Student has submitted proof of payment; awaiting finance confirmation.</summary>
    Submitted = 2,

    /// <summary>Finance has confirmed payment received; status is final.</summary>
    Paid = 3,

    /// <summary>Receipt was cancelled or voided.</summary>
    Cancelled = 4
}
