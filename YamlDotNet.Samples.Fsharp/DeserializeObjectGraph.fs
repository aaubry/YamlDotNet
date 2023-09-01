module YamlDotNet.Samples.Fsharp.DeserializeObjectGraph

open System
open System.Collections.Generic
open System.IO
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

[<CLIMutable>]
type Customer = { Given: string; Family: string }

[<CLIMutable>]
type OrderItem =
    { [<YamlMember(Alias = "part_no", ApplyNamingConventions = false)>]
      PartNo: string
      Descrip: string
      Price: decimal
      Quantity: int }

[<CLIMutable>]
type Address =
    { Street: string
      City: string
      State: string }

[<CLIMutable>]
type Order =
    { Receipt: string
      Date: DateTime
      Customer: Customer
      Items: List<OrderItem>

      [<YamlMember(Alias = "bill-to", ApplyNamingConventions = false)>]
      BillTo: Address

      [<YamlMember(Alias = "ship-to", ApplyNamingConventions = false)>]
      ShipTo: Address
      SpecialDelivery: string }

let Document =
    @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |-
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery: >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
..."

let main () =
    let input = new StringReader(Document)

    let deserializer =
        DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build()

    let order = deserializer.Deserialize<Order>(input)

    printfn "Order"
    printfn "-----"
    printfn ""

    order.Items.ForEach(fun item -> printfn $"{item.PartNo}\t{item.Quantity}\t{item.Price}\t{item.Descrip}")

    printfn ""

    printfn "Shipping"
    printfn "--------"
    printfn ""
    printfn "%A" order.ShipTo.Street
    printfn "%A" order.ShipTo.City
    printfn "%A" order.ShipTo.State
    printfn ""

    printfn "Billing"
    printfn "-------"
    printfn ""

    if (order.BillTo = order.ShipTo) then
        printfn "*same as shipping address*"
    else
        printfn "%A" order.ShipTo.Street
        printfn "%A" order.ShipTo.City
        printfn "%A" order.ShipTo.State

    printfn ""

    printfn "Delivery instructions"
    printfn "---------------------"
    printfn ""
    printfn "%A" order.SpecialDelivery

main ()
