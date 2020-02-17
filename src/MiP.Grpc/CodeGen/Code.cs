namespace MiP.Grpc.CodeGen
{
    internal static class Code
    {
        public const string GlobalPrefix = "global::";

        public const string ClassCode = @"{Attributes}
public class {Class} : {BaseClass}
{{Members}}
";

        public const string ConstructorCode = @"
    private readonly global::MiP.Grpc.IDispatcher _dispatcher;
    public {Constructor}(global::MiP.Grpc.IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
";

        public const string MethodHandlerCode = @"{Attributes}
    public async override
        global::System.Threading.Tasks.Task<{Response}>
        {Method}(
            {Request} request,
            global::Grpc.Core.ServerCallContext context
        )
    {
        return await _dispatcher.Dispatch<{Request}, {Response}, {Handler}>(request, context);
    }
";

        public const string MethodCommandHandlerCode = @"{Attributes}
    public async override
        global::System.Threading.Tasks.Task<global::Google.Protobuf.WellKnownTypes.Empty>
        {Method}(
            {Request} request,
            global::Grpc.Core.ServerCallContext context
        )
    {
        return await _dispatcher.Dispatch<
            {Request},
            global::Google.Protobuf.WellKnownTypes.Empty,
            global::MiP.Grpc.ICommandHandlerAdapter<{Request}, {Handler}>
            >(request, context);
    }
";

        public const string MethodServerStreamHandlerCode = @"{Attributes}
    public async override
        global::System.Threading.Tasks.Task
        {Method}(
            {Request} request,
            global::Grpc.Core.IServerStreamWriter<{Response}> responseStream,
            global::Grpc.Core.ServerCallContext context
        )
    {
        var streamRequest = new global::MiP.Grpc.Internal.ServerStreamRequest<{Request}, {Response}>(request, responseStream);
        
        await _dispatcher.Dispatch<
            global::MiP.Grpc.IServerStreamRequest<{Request}, {Response}>,
            global::Google.Protobuf.WellKnownTypes.Empty,
            global::MiP.Grpc.IServerStreamHandlerAdapter<{Request}, {Response}, {Handler}>
            >(streamRequest, context);
    }
";

        public const string MethodBidiStreamHandlerCode = @"{Attributes}
    public async override
        global::System.Threading.Tasks.Task
        {Method}(
            global::Grpc.Core.IAsyncStreamReader<{Request}> requestStream,
            global::Grpc.Core.IServerStreamWriter<{Response}> responseStream,
            global::Grpc.Core.ServerCallContext context
        )
    {
        var streamRequest = new global::MiP.Grpc.Internal.BidiStreamRequest<{Request}, {Response}>(requestStream, responseStream);
        
        await _dispatcher.Dispatch<
            global::MiP.Grpc.IBidiStreamRequest<{Request}, {Response}>,
            global::Google.Protobuf.WellKnownTypes.Empty,
            global::MiP.Grpc.IBidiStreamHandlerAdapter<{Request}, {Response}, {Handler}>
            >(streamRequest, context);
    }
";

        public const string MethodClientStreamHandlerCode = @"{Attributes}
    public async override
        global::System.Threading.Tasks.Task<{Response}>
        {Method}(
            global::Grpc.Core.IAsyncStreamReader<{Request}> streamRequest,
            global::Grpc.Core.ServerCallContext context
        )
    {
        return await _dispatcher.Dispatch<
            global::Grpc.Core.IAsyncStreamReader<{Request}>,
            {Response},
            global::MiP.Grpc.IClientStreamHandlerAdapter<{Request}, {Response}, {Handler}>
            >(streamRequest, context);
    }
";

        public const string ReturnTypeCode = @"
return typeof({Class});
";

        public const string AuthorizeAttributeCode = @"
    [global::Microsoft.AspNetCore.Authorization.Authorize({AttributeProperties})]";

        public const string PolicyPropertyCode = @"Policy = ""{Policy}""";
        public const string RolesPropertyCode = @"Roles = ""{Roles}""";
        public const string AuthenticationSchemesPropertyCode = @"AuthenticationSchemes = ""{AuthenticationSchemes}""";
    }
}
