#if USE_FCS
#r "nuget: FSharp.Compiler.Service, Version=43.12.101-preview7.26359.118"
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Text
open FSharp.Compiler.Syntax

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

let collectSynLongId list (longId: SynLongIdent) =
    match longId with
    | SynLongIdent(longId, _, _) ->
        collectLongId list longId

let collectConst list constant = 
    match constant with
    | SynConst.SourceIdentifier (ident, _, _) ->
        list @ [ident]
    | SynConst.Measure (_, _, synMeasure, _) ->
        printfn "Measure: %A" synMeasure
        list
    | _ -> list
let collectSynAttributes list (attrs: SynAttributes) =
    list

let rec collectType list (type': SynType) =
    match type' with
    | SynType.LongIdent(longId) ->
        collectSynLongId list longId
    | SynType.App(typeName, _, typeArgs, _, _, _, _) ->
        let list = collectType list typeName
        typeArgs |> List.fold collectType list
    | SynType.Tuple(_, types, _) ->
        types |> List.fold collectSynTupleTypeSegment list
    | SynType.Array(_, elemType, _) ->
        collectType list elemType
    | SynType.Fun(argType, returnType, _, _) ->
        let list = collectType list argType
        collectType list returnType
    | SynType.Anon (_) ->
        list
    | SynType.Var (synTypar, _) ->
        match synTypar with
        | SynTypar (ident, typarStaticReq, _) ->
            list @ [ident.idText]
    | SynType.Paren (innerType, _) ->
        collectType list innerType
    | SynType.SignatureParameter (attrs, _, id, usedType, _) ->
        let list = collectSynAttributes list attrs
        let list = match id with | Some ident -> list @ [ident.idText] | None -> list
        collectType list usedType
    | _ ->
        printfn "Type: %A" type'
        list

and collectSynTupleTypeSegment list (typeSegment: SynTupleTypeSegment) =
    match typeSegment with
    | SynTupleTypeSegment.Type(typeName) ->
        collectType list typeName
    | SynTupleTypeSegment.Star(_)
    | SynTupleTypeSegment.Slash(_) ->
        list

let rec collectSimplePatternList list pats =
    match pats with
    | SynSimplePats.SimplePats(pats, _, _) ->
        let list = pats |> List.fold collectSimplePattern list
        list

and collectSimplePattern list pats =
    match pats with
    | SynSimplePat.Id (ident, _, _, _, _, _) ->
        list @ [ident.idText]
    | SynSimplePat.Typed (pat, typeName, _) ->
        let list = collectSimplePattern list pat
        collectType list typeName
    | SynSimplePat.Attrib (pat, attrs, _) ->
        let list = collectSimplePattern list pat
        let list = collectSynAttributes list attrs
        list

let rec collectPattern list headPat =
    match headPat with
    | SynPat.LongIdent(longId, extraId, typarDecls, argPats, _, _) ->
        //printfn "Long id: %A" longId
        //printfn "Extra id: %A" extraId
        //printfn "Type parameters: %A" typarDecls
        //printfn "Argument patterns: %A" argPats
        //printfn "Accessibility: %A" accessibility
        let list = longId |> collectSynLongId list 
        let list = extraId |> Option.map (fun id -> list @ [id.idText]) |> Option.defaultValue list
        list
    | SynPat.Named (ident, _, _, _) ->
        match ident with
        | SynIdent(id, _) ->
            list @ [id.idText]
    | SynPat.Const (constant, _) ->
        collectConst list constant
    | SynPat.Wild _ ->
        list
    | SynPat.Tuple (_, pats, _, _) ->
        pats |> List.fold collectPattern list
    | SynPat.Paren (pat, _) ->
        collectPattern list pat
    | SynPat.Typed (pat, typeName, _) ->
        let list = collectPattern list pat
        collectType list typeName
    | _ ->
        printfn "Head Pattern: %A" headPat
        list
let collectIdentifiersFromBindings list binding =
    match binding with
    | SynBinding(_, _, _, _, attrs, _, valData, headPat, returnInfo, expr, _, _, _) ->
        //printfn "Access: %A" access
        //printfn "Kind: %A" kind
        //printfn "Is Inline: %b" isInline
        //printfn "Is Mutable: %b" isMutable
        //printfn "Attributes: %A" attrs
        let list = attrs |> collectSynAttributes list
        //printfn "XML Doc: %A" xmlDoc
        //printfn "Val Data: %A" valData
        let list = headPat |> collectPattern list 
        //printfn "Return Info: %A" returnInfo
        //printfn "Expression: %A" expr
        //printfn "Range: %A" range
        list

let rec collectIdentifiersFromExpr list expr =
    // Implementation for collecting identifiers from expressions
    match expr with
    | SynExpr.Ident ident ->
        //printfn "Identifier: %A" ident
        list @ [ident.idText]
    | SynExpr.LongIdent(_, longId, _, _) ->
        collectLongId list longId.LongIdent
    | SynExpr.LongIdentSet (longId, expr, _) ->
        let list = longId |> collectSynLongId list
        let list = expr |> collectIdentifiersFromExpr list
        list
    | SynExpr.App (_, _, funcExpr, argExpr, _) ->
        let list = collectIdentifiersFromExpr list funcExpr
        collectIdentifiersFromExpr list argExpr
    | SynExpr.Const (constant, _) ->
        collectConst list constant
    | SynExpr.InterpolatedString (parts, _, _) ->
        parts |> List.fold (fun acc part ->
            match part with
            | SynInterpolatedStringPart.String (text, _) -> acc
            | SynInterpolatedStringPart.FillExpr (expr, _) -> collectIdentifiersFromExpr acc expr
        ) list
    | SynExpr.Paren (expr, _, _, _) ->
        collectIdentifiersFromExpr list expr
    | SynExpr.Tuple (_, exprs, _, _) ->
        exprs |> List.fold collectIdentifiersFromExpr list
    | SynExpr.TypeApp (expr, _, typeArgs, _, _, _, _) ->
        let list = collectIdentifiersFromExpr list expr
        typeArgs |> List.fold collectType list
#if USE_FCS
    | SynExpr.LetOrUse synletUse ->
        let (bindings, bodyExpr) = (synletUse.Bindings, synletUse.Body)
#else
    | SynExpr.LetOrUse (_, _, bindings, bodyExpr, _, _) ->
#endif
        let list = bindings |> List.fold collectIdentifiersFromBindings list
        collectIdentifiersFromExpr list bodyExpr
    | SynExpr.Match (_, expr, clauses, _, _) ->
        let list = collectIdentifiersFromExpr list expr
        clauses |> List.fold (fun acc clause ->
            match clause with
            | SynMatchClause(pat, whenExprOpt, resultExpr, _, _, _) ->
                let acc = collectPattern acc pat
                let acc = 
                    match whenExprOpt with
                    | Some whenExpr -> collectIdentifiersFromExpr acc whenExpr
                    | None -> acc
                collectIdentifiersFromExpr acc resultExpr
        ) list
    | SynExpr.IfThenElse (ifExpr, thenExpr, elseExprOpt, _, _, _, _) ->
        let list = collectIdentifiersFromExpr list ifExpr
        let list = collectIdentifiersFromExpr list thenExpr
        match elseExprOpt with
        | Some elseExpr -> collectIdentifiersFromExpr list elseExpr
        | None -> list
    | SynExpr.For (_, _, ident, _, identBody, _, toBodyExpr, doBodyExpr,_) ->
        let list = list @ [ident.idText]
        let list = collectIdentifiersFromExpr list identBody
        let list = collectIdentifiersFromExpr list toBodyExpr
        collectIdentifiersFromExpr list doBodyExpr
    | SynExpr.While (_, whileExpr, doBodyExpr, _) ->
        let list = collectIdentifiersFromExpr list whileExpr
        collectIdentifiersFromExpr list doBodyExpr
    | SynExpr.ArrayOrList (_, exprs, _) ->
        exprs |> List.fold collectIdentifiersFromExpr list
    | SynExpr.Lambda (_, _, pats, bodyExpr, parsedData, _, _) ->
        let list = collectSimplePatternList list pats
        collectIdentifiersFromExpr list bodyExpr
    | SynExpr.ArrayOrListComputed (_, expr, _) ->
        collectIdentifiersFromExpr list expr
    | SynExpr.Sequential (_, _, expr1, expr2, _, _) ->
        let list = collectIdentifiersFromExpr list expr1
        collectIdentifiersFromExpr list expr2
    | SynExpr.DotIndexedGet (expr, indexExprs, _, _) ->
        let list = collectIdentifiersFromExpr list expr
        collectIdentifiersFromExpr list indexExprs
    | SynExpr.DotIndexedSet (objExpr, indexExprs, valueExpr, _, _, _) ->
        let list = collectIdentifiersFromExpr list objExpr
        let list = collectIdentifiersFromExpr list valueExpr
        collectIdentifiersFromExpr list indexExprs
    | SynExpr.DotGet (expr, _, longId, _) ->
        let list = collectIdentifiersFromExpr list expr
        collectSynLongId list longId
    | SynExpr.DotSet (expr, longId, valueExpr, _) ->
        let list = collectIdentifiersFromExpr list expr
        let list = collectSynLongId list longId
        collectIdentifiersFromExpr list valueExpr
    | SynExpr.ForEach (_, _, _, _, pats, expr, bodyExpr, _) ->
        let list = collectPattern list pats
        let list = collectIdentifiersFromExpr list expr
        collectIdentifiersFromExpr list bodyExpr
    | _ ->
        // Handle other expression types as needed
        printfn "Expression: %A" expr
        list

let rec collectSynMemberDefn list memberDef =
    match memberDef with
    | SynMemberDefn.Member(binding, _) ->
        collectIdentifiersFromBindings list binding
#if USE_FCS
    | SynMemberDefn.LetBindings (bindings, _, _, _, _) ->
#else
    | SynMemberDefn.LetBindings (bindings, _, _, _) ->
#endif
        bindings |> List.fold collectIdentifiersFromBindings list
    | SynMemberDefn.ImplicitCtor (_, attrs, ctorArgs, selfIdentifier, _, _, _) ->
        let acc = collectSynAttributes list attrs
        let acc = collectPattern acc ctorArgs
        match selfIdentifier with
        | Some ident -> acc @ [ident.idText]
        | None -> acc
#if USE_FCS
    | SynMemberDefn.ImplicitInherit (typeName, expr, identOpt, _, _) ->
#else
    | SynMemberDefn.ImplicitInherit (typeName, expr, identOpt, _) ->
#endif
        let acc = collectType list typeName
        let acc = collectIdentifiersFromExpr acc expr
        match identOpt with
        | Some ident -> acc @ [ident.idText]
        | None -> acc
    | SynMemberDefn.AbstractSlot (valSig, _, _, _) ->
        match valSig with
        | SynValSig.SynValSig(attr, ident, explicitTypeParams, synType, _, _, _, _, _, synExpr, _, _) ->
            let acc = attr |> collectSynAttributes list
            let acc = match ident with | SynIdent(id, _) -> acc @ [id.idText]
            let acc = collectType acc synType
            let acc = match synExpr with | Some expr -> collectIdentifiersFromExpr acc expr | None -> acc
            acc
    | SynMemberDefn.ValField (field, _) ->
        match field with
        | SynField(attrs, _, identOpt, synType, _, _, _, _, _) ->
            let acc = attrs |> collectSynAttributes list
            let acc = match identOpt with | Some ident -> acc @ [ident.idText] | None -> acc
            let acc = collectType acc synType
            acc
    | SynMemberDefn.Interface (typeName, _, membersOpt, _) ->
        let acc = collectType list typeName
        match membersOpt with
        | Some members -> members |> List.fold collectSynMemberDefn acc
        | None -> acc
#if USE_FCS
    | SynMemberDefn.Inherit (baseType, asIdent, _, _) ->
        let list = match baseType with | Some baseType -> collectType list baseType | None -> list
#else
    | SynMemberDefn.Inherit (baseType, asIdent, _) ->
        let list = collectType list baseType
#endif
        match asIdent with
        | Some ident -> list @ [ident.idText]
        | None -> list
    | SynMemberDefn.AutoProperty (attrs, isStatic, ident, typeOpt, _, _, _, _, _, synExpr, _, _) ->
        let list = collectSynAttributes list attrs
        let list = list @ [ident.idText]
        let list = 
            match typeOpt with
            | Some synType -> collectType list synType
            | None -> list
        collectIdentifiersFromExpr list synExpr
    | _ -> 
        printfn "Member definition: %A" memberDef
        list

let rec collectIdentifiersFromDecls list decl =
    match decl with
#if USE_FCS
    | SynModuleDecl.Let(isRecursive, bindings, range, _) ->
#else
    | SynModuleDecl.Let(isRecursive, bindings, range) ->
#endif
        bindings |> List.fold collectIdentifiersFromBindings list
    | SynModuleDecl.NestedModule (moduleInfo, _, decls, _, _, _) ->
        let list = 
            match moduleInfo with
            | SynComponentInfo(attrs, typeParams, constraints, longId, _, preferPostfix, access, range) ->
                let list = collectSynAttributes list attrs
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
    | SynModuleDecl.Expr (expr, _) ->
        //printfn "Range: %A" range
        collectIdentifiersFromExpr list expr
    | SynModuleDecl.Types (typeDefs, _) ->
        typeDefs |> List.fold (fun acc typeDef ->
            match typeDef with
            | SynTypeDefn(typeInfo, typeRepr, members, implicitCtor, _, _) ->
                let acc = 
                    match typeInfo with
                    | SynComponentInfo(attrs, typeParams, constraints, longId, _, preferPostfix, access, _) ->
                        let acc = attrs |> collectSynAttributes acc
                        //printfn "Type Parameters: %A" typeParams
                        //printfn "Constraints: %A" constraints
                        //printfn "Long Id: %A" longId
                        collectLongId acc longId
                        //printfn "XML Doc: %A" xmlDoc
                        //printfn "Prefer Postfix: %b" preferPostfix
                        //printfn "Access: %A" access
                        //printfn "Range: %A" range
                let acc = 
                    match typeRepr with
                    | SynTypeDefnRepr.ObjectModel(kind, members, _) ->
                        members |> List.fold collectSynMemberDefn acc
                    | SynTypeDefnRepr.Simple(simpleRepr, _) ->
                        match simpleRepr with
                        | SynTypeDefnSimpleRepr.Record (_, fields, _) ->
                            fields |> List.fold (fun acc field ->
                                match field with
                                | SynField(attrs, _, identOpt, synType, _, _, _, _, _) ->
                                    let acc = attrs |> collectSynAttributes acc
                                    let acc = match identOpt with | Some ident -> acc @ [ident.idText] | None -> acc
                                    let acc = collectType acc synType
                                    acc
                            ) acc
                        | SynTypeDefnSimpleRepr.Union (_, cases, _) ->
                            cases |> List.fold (fun acc case ->
                                match case with
                                | SynUnionCase(attrs, ident, caseType, _, _, _, _) ->
                                    let acc = attrs |> collectSynAttributes acc
                                    let acc = match ident with | SynIdent(id, _) -> acc @ [id.idText]
                                    let acc = 
                                        match caseType with
                                        | SynUnionCaseKind.Fields fields ->
                                            fields |> List.fold (fun acc field ->
                                                match field with
                                                | SynField(attrs, _, identOpt, synType, _, _, _, _, _) ->
                                                    let acc = attrs |> collectSynAttributes acc
                                                    let acc = match identOpt with | Some ident -> acc @ [ident.idText] | None -> acc
                                                    let acc = collectType acc synType
                                                    acc
                                            ) acc
                                        | SynUnionCaseKind.FullType (synType, synValInfo) ->
                                            collectType acc synType
                                    acc
                            ) acc
                        | SynTypeDefnSimpleRepr.TypeAbbrev (_, synType, _) ->
                            collectType acc synType
                        | SynTypeDefnSimpleRepr.Enum (cases, _) ->
                            cases |> List.fold (fun acc case ->
                                match case with
                                | SynEnumCase(attrs, ident, synExpr, _, _, _) ->
                                    let acc = attrs |> collectSynAttributes acc
                                    let acc = match ident with | SynIdent(id, _) -> acc @ [id.idText]
                                    let acc = collectIdentifiersFromExpr acc synExpr
                                    acc
                            ) acc
                        | _ -> 
                            printfn "Simple representation: %A" simpleRepr
                            acc
                    | _ -> 
                        printfn "Type representation: %A" typeRepr
                        acc
                acc
        ) list
    | SynModuleDecl.Open (target, _) ->
        match target with
        | SynOpenDeclTarget.ModuleOrNamespace (longId, _) ->
            collectSynLongId list longId
        | SynOpenDeclTarget.Type (typeName, _) ->
            collectType list typeName
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
                    let acc = decls |> List.fold collectIdentifiersFromDecls acc
                    //printfn "Declarations: %A" decls
                    //printfn "XML Doc: %A" xmlDoc
                    let acc = attrs |> collectSynAttributes acc
                    //printfn "Access: %A" access
                    //printfn "Range: %A" range
                    acc
            ) list
            //printfn "Is Exe: %b" isExe
            //printfn "Is Last Compiland: %b" isLastCompiland
            //printfn "Trivia: %A" trivia
            //printfn "Identifiers: %A" identifiers
    | _ ->
        printfn "%A" ast
        list
open System.IO
let collectIdentifiersFromFile list fileName =
    let code = File.ReadAllText fileName
    let ast = parseToAST code
    collectIdentifiers list ast

if fsi.CommandLineArgs.Length = 0 then
    printfn "Usage: fsi identifiers.fsx <basePath/file>"
else
    let basePath = fsi.CommandLineArgs.[1]
    let fileNames = 
        if Directory.Exists(basePath) then
            let sampleFiles = [
                "tour/primitives.fs"
                "tour/functions.fs"
                "tour/collections.fs"
                "tour/records.fs"
                "tour/unions.fs"
                "tour/units.fs"
                "tour/classes.fs"

                "visual/basic-canvas.fs"
                "visual/mandelbrot.fs"
                "visual/raytracer.fs"
                "visual/hokusai.fs"
                "visual/color-fountain.fs"
                "visual/fractal.fs"

                "games/undertone.fs"
                "games/ants.fs"
                "games/mario.fs"
                "games/ozmo.fs"
                "games/pacman.fs"
    
                "ui/spreadsheet.fs"
                "ui/webcomponent.fs"
            ]
            sampleFiles |> List.map (fun fileName -> Path.Combine(basePath, "samples", fileName))
        else
            [basePath]
    let identifiers =
        fileNames |> List.fold collectIdentifiersFromFile []

    printfn "Identifiers: %A" (identifiers |> List.distinct)