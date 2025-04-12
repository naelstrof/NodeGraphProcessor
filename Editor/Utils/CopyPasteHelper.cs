using System;
using System.Collections.Generic;

namespace GraphProcessor {
[Serializable]
public class CopyPasteHelper {
    public List<JsonElement> copiedNodes = new();

    public List<JsonElement> copiedGroups = new();

    public List<JsonElement> copiedEdges = new();
}
}