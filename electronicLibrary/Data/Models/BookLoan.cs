namespace electronicLibrary.Data.Models
{
    public class BookLoan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public  Book? Book { get; set; }
        public string? UserId { get; set; }
        public  ApplicationUser? User { get; set; }
        private DateTime _loanDate;
        public DateTime LoanDate
        {
            get => _loanDate;
            set => _loanDate = value <= DateTime.Now ? value : throw new ArgumentException("Loan date cannot be in the future");
        }
        private DateTime? _returnDate;
        public DateTime? ReturnDate
        {
            get => _returnDate;
            set => _returnDate = value > LoanDate ? value : throw new ArgumentException("Return date cannot be before loan date");
        }
        public DateTime DueDate { get; set; }
        public bool IsOverdue => DateTime.Now > DueDate && !ReturnDate.HasValue;
    }
}
