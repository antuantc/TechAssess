// Monaco editor + Roslyn IntelliSense bridge for the interview console.
// Loads Monaco from the CDN AMD bundle, wires a completion provider and live
// diagnostics back to the Blazor circuit via a .NET object reference.
window.monacoInterop = (function () {
    let editor = null;
    let dotNetRef = null;
    let diagnosticsTimer = null;
    let providerRegistered = false;

    function mapCompletionKind(kind) {
        const k = monaco.languages.CompletionItemKind;
        switch (kind) {
            case 'Method': return k.Method;
            case 'Property': return k.Property;
            case 'Field': return k.Field;
            case 'Variable': return k.Variable;
            case 'Class': return k.Class;
            case 'Interface': return k.Interface;
            case 'Enum': return k.Enum;
            case 'EnumMember': return k.EnumMember;
            case 'Struct': return k.Struct;
            case 'Function': return k.Function;
            case 'Module': return k.Module;
            case 'Keyword': return k.Keyword;
            case 'Event': return k.Event;
            case 'Constant': return k.Constant;
            default: return k.Text;
        }
    }

    function registerProviders() {
        if (providerRegistered) {
            return;
        }
        providerRegistered = true;

        monaco.languages.registerCompletionItemProvider('csharp', {
            triggerCharacters: ['.', ' '],
            provideCompletionItems: async function (model, position) {
                if (!dotNetRef) {
                    return { suggestions: [] };
                }

                const code = model.getValue();
                const offset = model.getOffsetAt(position);
                let entries;
                try {
                    entries = await dotNetRef.invokeMethodAsync('ProvideCompletions', code, offset);
                } catch {
                    return { suggestions: [] };
                }

                const word = model.getWordUntilPosition(position);
                const range = {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn,
                    endColumn: word.endColumn
                };

                const suggestions = entries.map(function (e) {
                    return {
                        label: e.label,
                        kind: mapCompletionKind(e.kind),
                        insertText: e.insertText,
                        detail: e.detail,
                        range: range
                    };
                });

                return { suggestions: suggestions };
            }
        });
    }

    function scheduleDiagnostics() {
        if (diagnosticsTimer) {
            clearTimeout(diagnosticsTimer);
        }
        diagnosticsTimer = setTimeout(runDiagnostics, 600);
    }

    async function runDiagnostics() {
        if (!editor || !dotNetRef) {
            return;
        }
        const model = editor.getModel();
        if (!model) {
            return;
        }

        let markers;
        try {
            markers = await dotNetRef.invokeMethodAsync('ProvideDiagnostics', model.getValue());
        } catch {
            return;
        }

        monaco.editor.setModelMarkers(model, 'roslyn', markers.map(function (m) {
            return {
                message: m.message,
                severity: m.severity,
                startLineNumber: m.startLineNumber,
                startColumn: m.startColumn,
                endLineNumber: m.endLineNumber,
                endColumn: m.endColumn
            };
        }));
    }

    return {
        init: function (elementId, ref, initialCode, language) {
            language = language || 'csharp';
            dotNetRef = ref;
            return new Promise(function (resolve) {
                require(['vs/editor/editor.main'], function () {
                    if (language === 'csharp') {
                        registerProviders();
                    }
                    editor = monaco.editor.create(document.getElementById(elementId), {
                        value: initialCode,
                        language: language,
                        theme: 'vs-dark',
                        automaticLayout: true,
                        fontSize: 14,
                        minimap: { enabled: false },
                        scrollBeyondLastLine: false,
                        tabSize: language === 'sql' ? 2 : 4,
                        renderWhitespace: 'selection'
                    });

                    if (language === 'csharp') {
                        editor.onDidChangeModelContent(scheduleDiagnostics);
                        scheduleDiagnostics();
                    }
                    resolve();
                });
            });
        },

        setValue: function (code) {
            if (editor) {
                editor.setValue(code);
                const model = editor.getModel();
                if (model) {
                    monaco.editor.setModelMarkers(model, 'roslyn', []);
                }
                scheduleDiagnostics();
            }
        },

        getValue: function () {
            return editor ? editor.getValue() : '';
        },

        dispose: function () {
            if (diagnosticsTimer) {
                clearTimeout(diagnosticsTimer);
                diagnosticsTimer = null;
            }
            if (editor) {
                editor.dispose();
                editor = null;
            }
            dotNetRef = null;
        }
    };
})();
