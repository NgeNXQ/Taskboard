using System;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Common;

public interface IDatabaseExceptionTranslator
{
    void ThrowInnerException(Exception? exception);
}
