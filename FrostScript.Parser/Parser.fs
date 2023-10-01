﻿namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let appendToLastInState token tokens isBlock =
            let current = tokens |> List.last
            (tokens |> List.updateAt (tokens.Length - 1) (List.append current [token]), isBlock)

        tokens
        |> List.fold (fun state token -> 
            let (tokens, isBlock) = state
            match token.Type with
            | SemiColon  -> 
                if isBlock then appendToLastInState token tokens isBlock
                else (List.append tokens [[]], isBlock)
            | Pipe       -> appendToLastInState token tokens true
            | ReturnPipe -> appendToLastInState token tokens false
            | _          -> appendToLastInState token tokens isBlock
        ) ([[]], false)
        |> fst
        |> List.where (fun x -> not x.IsEmpty)
        |> List.map (fun tokens ->
            let (node, _) = Functions.expression tokens
            node
        )