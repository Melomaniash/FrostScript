﻿namespace FrostScript

type DataType =
| AnyType
| NumberType
| BoolType
| StringType
| FunctionType of Input : DataType * Output : DataType
| VoidType
| ObjectType of Fields : Map<string, DataType>
with override this.ToString() = 
        match this with
        | AnyType -> "any"
        | NumberType -> "num"
        | BoolType -> "bool"
        | StringType -> "string"
        | FunctionType (input, output)-> $"{input} -> {output}"
        | VoidType -> "void"
        | ObjectType (fields) ->  
            let fields = 
                fields
                |> Map.toList 
                |> List.map (fun (id, value) -> $"{id} {value}")
            "{ " + String.concat ", " fields + " }"

and Paramater =
    { Id : string
      Value : DataType }