using System.Runtime;

namespace BasketService.Base;

public class ResponseModel<T>{
    public bool isSuccess {get;set;}=false;
    public string Message { get; set; } 
    public T Data { get; set; }
    public long index{get;set;}
}