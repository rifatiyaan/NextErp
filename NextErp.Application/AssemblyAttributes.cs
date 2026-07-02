using System.Runtime.CompilerServices;

// Mapperly mappers are internal static partial classes; these grants let
// the API, Infrastructure, and test assemblies call the generated
// extension methods (e.g. request.ToEntity(), entity.ToResponse()).
[assembly: InternalsVisibleTo("NextErp.API")]
[assembly: InternalsVisibleTo("NextErp.Infrastructure")]
[assembly: InternalsVisibleTo("NextErp.Application.Tests")]
