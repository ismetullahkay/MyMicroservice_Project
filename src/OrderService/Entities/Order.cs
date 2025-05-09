using System.ComponentModel.DataAnnotations;

namespace OrderService.Entities;

public class Order
{
    [Key] //1 1 artmasını sağlar her kayıt için
    public int OrderId { get; set; }
    public Guid GameId { get; set; }
    public string GameName { get; set; }
    public string GameAuthor { get; set; }

    public decimal Price { get; set; }  
    
    public string GameDescription { get; set; } 
    public Guid UserId { get; set; } 
}