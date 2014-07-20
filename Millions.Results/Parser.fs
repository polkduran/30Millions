module Parser
open System
open System.IO
open System.Text.RegularExpressions
(*

true|false
proxy

1st,2nd nav

Result ok nb votes

*)

type Proxy = Proxy of string
type Votes = Votes of int
type Navigations = Navigation of int

type Result = 
    |NotOk of Proxy * Navigations
    |Ok of Proxy * Votes
    |OkNoVote of Proxy * Navigations


let parseResults (file:string) = 

    let toResult (lines:string list) = 
        let ok, rest = match lines with
                          | "True"::r -> true,r
                          | "False"::r -> false,r
                          | _ -> failwith "First line format not ok"
        
        let (|Pxy|_|) line = 
             let res = Regex.Match(line, "Using proxy http://(?<pxy>.+$)").Groups.["pxy"].Value
             res |> function |""->None |_->Some(res)
         
        let (|Nav|_|) line = 
            if Regex.IsMatch(line, "^Start request ") then Some(true)
            elif Regex.IsMatch(line, "^End request ") then Some(false)
            else None

        let (|Votes|_|) line =
            let groups = Regex.Match(line, "\{\"message\":\"(?<m>[^\"]*)\",\"nbvotes\":\"(?<v>\d+)\"\}").Groups
            let voteOk,votesStr = groups.["m"].Value = "", groups.["v"].Value

            Int32.TryParse(votesStr)
                |> function |(true,votes) -> Some(voteOk, votes) |_ -> None



        let rec parse state = 
                    let p,nav,votes,hasVoted = state
                    function
                    | Pxy(pxy)::r ->                              
                                parse (pxy,nav,votes,hasVoted) r
                    | Nav(true)::r ->
                                parse (p,nav,votes,hasVoted) r
                    | Nav(false)::r ->
                                parse (p,nav+1,votes,hasVoted) r
                    | Votes(voteOk, nbVotes)::r ->
                                parse (p,nav,nbVotes,voteOk) r
                    | [] -> state
                    |_::r -> parse (p,nav,votes,hasVoted) r

        let proxy,nbNavs,nbVotes,hasVoted = parse ("",0,0,false) rest
        match ok,hasVoted with
            |true,true -> Result.Ok(Proxy(proxy),Votes(nbVotes))
            |false,_ -> Result.NotOk(Proxy(proxy),Navigation(nbNavs))
            |true,false -> Result.OkNoVote(Proxy(proxy),Navigation(nbNavs))
        


    use reader = new StreamReader(file)
    let rec parse (reader:StreamReader) acc inState =
        seq{
            match reader.ReadLine() with
            | null -> if inState then yield acc
            | "#-#-#" -> if inState then yield acc
                         yield! parse reader [] (not inState)
            | line -> yield! parse reader (line::acc) true
        }
    parse reader [] false
    |> Seq.map (List.rev >> toResult)
    |> List.ofSeq



