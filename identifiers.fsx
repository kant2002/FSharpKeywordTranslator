#if USE_FCS
//#r "nuget: FSharp.Core, Version=10.1.204"
//#r "nuget: FSharp.Compiler.Service, Version=43.12.101-preview7.26359.118"
//open FSharp.Compiler.CodeAnalysis
//open FSharp.Compiler.Text
//open FSharp.Compiler.Syntax

let parseToAST (inputCode: string) =
    let checker = FSharpChecker.Create()
    
    let filename = "file.fsx"
    let sourceText = SourceText.ofString inputCode
    
    let opts = { FSharpParsingOptions.Default with SourceFiles = [| filename |] }

    let parseResults = 
        checker.ParseFile(filename, sourceText, opts) 
        |> Async.RunSynchronously

    if parseResults.ParseHadErrors then
        printfn "Errors: %A" parseResults.Diagnostics
        
    parseResults.ParseTree
#else
#r "nuget: Fantomas.FCS"

open Fantomas.FCS.Text
open Fantomas.FCS.Syntax

let parseToAST (inputCode: string) =
    let ast =
        Fantomas.FCS.Parse.parseFile false (Fantomas.FCS.Text.SourceText.ofString inputCode) []
        |> fst
    ast
#endif

let collectLongId list (longId: LongIdent) =
    list @ (longId |> List.map (fun id -> id.idText))

let collectIdentifiersFromBindings list binding =
    match binding with
    | SynBinding(access, kind, isInline, isMutable, attrs, xmlDoc, valData, headPat, returnInfo, expr, range, _, trivia) ->
        //printfn "Access: %A" access
        //printfn "Kind: %A" kind
        //printfn "Is Inline: %b" isInline
        //printfn "Is Mutable: %b" isMutable
        //printfn "Attributes: %A" attrs
        //printfn "XML Doc: %A" xmlDoc
        //printfn "Val Data: %A" valData
        match headPat with
        | SynPat.LongIdent(longId, extraId, typarDecls, argPats, accessibility, _) ->
            printfn "Long id: %A" longId
            printfn "Extra id: %A" extraId
            printfn "Type parameters: %A" typarDecls
            printfn "Argument patterns: %A" argPats
            printfn "Accessibility: %A" accessibility
            list
        | SynPat.Named (ident, isThisVar, access, range) ->
            match ident with
            | SynIdent(id, _) ->
                //printfn "Identifier: %s" (id.ToString())
                list @ [id.ToString()]
            //printfn "Identifier: %A" ident
            //printfn "Is this variable: %b" isThisVar
            //printfn "Access: %A" access
        | _ ->
            printfn "Head Pattern: %A" headPat
            list
        //printfn "Return Info: %A" returnInfo
        //printfn "Expression: %A" expr
        //printfn "Range: %A" range

let rec collectIdentifiersFromExpr list expr =
    // Implementation for collecting identifiers from expressions
    match expr with
    | SynExpr.Ident ident ->
        //printfn "Identifier: %A" ident
        list @ [ident.idText]
    | SynExpr.LongIdent(_, longId, _, _) ->
        collectLongId list longId.LongIdent
    | SynExpr.App (_, _, funcExpr, argExpr, _) ->
        let list = collectIdentifiersFromExpr list funcExpr
        collectIdentifiersFromExpr list argExpr
    | SynExpr.Const (constant, _) ->
        //printfn "Constant: %A" constant
        match constant with
        | SynConst.Measure (_, _, synMeasure, _) ->
            printfn "Measure: %A" synMeasure
            list
        | _ -> list
    | SynExpr.InterpolatedString (parts, _, _) ->
        parts |> List.fold (fun acc part ->
            match part with
            | SynInterpolatedStringPart.String (text, _) -> acc
            | SynInterpolatedStringPart.FillExpr (expr, _) -> collectIdentifiersFromExpr acc expr
        ) list
    | _ ->
        // Handle other expression types as needed
        printfn "Expression: %A" expr
        list

let rec collectIdentifiersFromDecls list decl =
    match decl with
#if USE_FCS
    | SynModuleDecl.Let(isRecursive, bindings, range, _) ->
#else
    | SynModuleDecl.Let(isRecursive, bindings, range) ->
#endif
        bindings |> List.fold collectIdentifiersFromBindings list
    | SynModuleDecl.NestedModule (moduleInfo, isRecursive, decls, isContinuing, range, _) ->
        let list = 
            match moduleInfo with
            | SynComponentInfo(attrs, typeParams, constraints, longId, xmlDoc, preferPostfix, access, range) ->
                //printfn "Attributes: %A" attrs
                //printfn "Type Parameters: %A" typeParams
                //printfn "Constraints: %A" constraints
                //printfn "Long Id: %A" longId
                collectLongId list longId
                //printfn "XML Doc: %A" xmlDoc
                //printfn "Prefer Postfix: %b" preferPostfix
                //printfn "Access: %A" access
                //printfn "Range: %A" range
        //printfn "Is Recursive: %b" isRecursive
        decls |> List.fold collectIdentifiersFromDecls list
    | SynModuleDecl.Expr (expr, range) ->
        
        //printfn "Range: %A" range
        collectIdentifiersFromExpr list expr
    | _ -> 
        printfn "Declaration: %A" decl
        list

let collectIdentifiers list (ast: ParsedInput) =
    match ast with
    | ParsedInput.ImplFile parsedInput ->
        match parsedInput with
#if USE_FCS
        | ParsedImplFileInput(fileName, isScript, qualifiedNameOfFile, hashDirectives, contents, (isExe, isLastCompiland), trivia, identifiers) ->
#else
        | ParsedImplFileInput(fileName, isScript, qualifiedNameOfFile, _, hashDirectives, contents, (isExe, isLastCompiland), trivia, identifiers) ->
#endif
            //printfn "File Name: %s" fileName
            //printfn "Is Script: %b" isScript
            //printfn "Qualified Name: %A" qualifiedNameOfFile
            //printfn "Hash Directives: %A" hashDirectives
            contents |> List.fold (fun acc moduleOrNamespace -> 
                match moduleOrNamespace with
                | SynModuleOrNamespace(longId, isRecursive, kind, decls, xmlDoc, attrs, access, range, _) ->
                    //printfn "Module/Namespace: %A" longId
                    //printfn "Is Recursive: %b" isRecursive
                    //printfn "Kind: %A" kind
                    let acc = acc @ (longId |> List.map (fun id -> id.idText))
                    decls |> List.fold collectIdentifiersFromDecls acc
                    //printfn "Declarations: %A" decls
                    //printfn "XML Doc: %A" xmlDoc
                    //printfn "Attributes: %A" attrs
                    //printfn "Access: %A" access
                    //printfn "Range: %A" range
            ) list
            //printfn "Is Exe: %b" isExe
            //printfn "Is Last Compiland: %b" isLastCompiland
            //printfn "Trivia: %A" trivia
            //printfn "Identifiers: %A" identifiers
    | _ ->
        printfn "%A" ast
        list

open System.IO
let code = File.ReadAllText fsi.CommandLineArgs.[1]
let ast = parseToAST code

printfn "Identifiers: %A" (collectIdentifiers [] ast |> List.distinct)