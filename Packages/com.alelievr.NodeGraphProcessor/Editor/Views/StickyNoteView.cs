#if UNITY_2020_1_OR_NEWER
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class StickyNoteView : UnityEditor.Experimental.GraphView.StickyNote {
    private ColorField colorField;
    public StickyNote note;
    public BaseGraphView owner;

    private Label titleLabel;

    public StickyNoteView() {
        fontSize = StickyNoteFontSize.Small;
        theme = StickyNoteTheme.Classic;
    }

    public void Initialize(BaseGraphView graphView, StickyNote note) {
        this.note = note;
        owner = graphView;

        this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => { note.title = e.newValue; });
        this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e => { note.content = e.newValue; });

        title = note.title;
        contents = note.content;
        SetPosition(note.position);
    }

    public override void SetPosition(Rect newPos) {
        base.SetPosition(newPos);

        if (note != null)
            note.position = newPos;
    }

    public override void OnResized() {
        note.position = layout;
    }
}
}
#endif