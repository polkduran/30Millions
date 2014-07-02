module Parser
open System
open System.IO

(*
proxy

OkNoVote -> 
    1st, 2nd navigation

OkVote -> (2 navs) nb votes
*)


type Result = 
    | NotOk
    | OkVote
    | OkNoVote


let parseResults (path:string) = 
    
    let (|Line|_|) (reader:StreamReader) = 
        match reader.ReadLine() with
        | null -> None
        | line -> Some(line)

    let rec readNext (reader:StreamReader) acc inState = 
        seq{
            match reader with
            | Line("#-#-#") -> if inState then yield acc
                               yield! readNext reader [] (not inState)
            | Line(line) -> yield! readNext reader (line::acc) true
            | _ -> if inState then yield acc
        }
    
    use fileReader = new StreamReader(path)
    let results = readNext fileReader [] false
    results 
            |> Seq.map List.rev
            |> List.ofSeq



    
    






