using System.ComponentModel.DataAnnotations;
namespace TicketSupportSystem.Models;

public enum TicketQuery
    {
        [Display(Name = "General Question")]
        GeneralQuestion = 0, // L1 scope
        Billing = 1, // L1 Scope
        Account = 2, // L2 Scope (plus everything L1 has as well)
        Product = 3, // L2 Scope (plus everything L1 has as well)
        [Display(Name = "Feedback / Complaint")]
        FeedbackComplaint = 4, // L2 Scope
        Sales = 5 // L1 Scope
    }