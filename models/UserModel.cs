public class Holding
{
    public string Symbol { get; set; } = string.Empty; 
    public decimal Amount { get; set; } 
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
   
    public string PasswordHash { get; set; } = string.Empty;
   
    public List<Holding> Holdings { get; set; } = new List<Holding>();
    public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();


}

public class PaymentMethod
{
    public string Method { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
}