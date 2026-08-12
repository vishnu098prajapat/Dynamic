using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Product
/// </summary>
public class Product
{
    public Product()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


public class AddToCart
{
    public int Msrno { get; set; }
    public int ID { get; set; }
    public string Img { get; set; }
    public int Quantity { get; set; }
    public int Amount { get; set; }
    public string Name { get; set; }
}



public class OrderItem
{
    public int ID { get; set; }
    public int Msrno { get; set; }
    public int PID { get; set; }
    public int order_id { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int Quantity { get; set; }
    public string Address { get; set; }
    public string Status { get; set; }
    public decimal NetAmount { get; set; }
    public string Content { get; set; }
   


}



public class OrderItemFinal
{
    public int ID { get; set; }
    public int Msrno { get; set; }
    public string Img { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public string Name { get; set; }

}







