// Plain textarea syntax highlighting for the interview console.
window.csharpEditor = (function () {
    const editorSettings = {
        'csharp-editor': { indent: '    ', comment: '//' },
        'sql-editor': { indent: '  ', comment: '--' }
    };
    const keywords = /\b(?:abstract|as|async|await|base|break|case|catch|class|const|continue|default|delegate|do|else|event| explicit|extern|false|finally|for|foreach|if|implicit|in|interface|internal|is|lock|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sealed|static|struct|switch|this|throw|true|try|typeof|using|virtual|void|while|var)\b/g;
    const types = /\b(?:bool|byte|char|decimal|double|float|int|long|short|string|Task|IEnumerable|IReadOnlyList|List|Dictionary|ArgumentException|NotImplementedException)\b/g;
    const numbers = /\b\d+(?:\.\d+)?\b/g;

    function escapeHtml(value) {
        return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function placeholder(index) {
        let key = '';
        do {
            key = String.fromCharCode(65 + (index % 26)) + key;
            index = Math.floor(index / 26) - 1;
        } while (index >= 0);
        return `\u0000${key}\u0000`;
    }

    function highlight(code) {
        let result = escapeHtml(code);
        const tokens = [];

        result = result.replace(/(\/\/[^\n]*|\/\*[\s\S]*?\*\/)/g, value => {
            tokens.push(`<span class="syntax-comment">${value}</span>`);
            return placeholder(tokens.length - 1);
        });
        result = result.replace(/(&quot;|&quot;)?("(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*')/g, value => {
            tokens.push(`<span class="syntax-string">${value}</span>`);
            return placeholder(tokens.length - 1);
        });
        result = result.replace(keywords, '<span class="syntax-keyword">$&</span>');
        result = result.replace(types, '<span class="syntax-type">$&</span>');
        result = result.replace(numbers, '<span class="syntax-number">$&</span>');
        return result.replace(/\u0000([A-Z]+)\u0000/g, (_, key) => {
            let index = 0;
            for (const character of key) {
                index = index * 26 + character.charCodeAt(0) - 64;
            }
            return tokens[index - 1];
        });
    }

    function syncCsharp() {
        const editor = document.getElementById('csharp-editor');
        const code = document.querySelector('#csharp-highlight code');
        if (!editor || !code) {
            return;
        }

        const restored = restoreEditor(editor);
        editor.closest('.code-editor-shell')?.classList.add('highlight-enabled');
        code.innerHTML = `${highlight(editor.value)}\n`;
        editor.onscroll = () => {
            code.parentElement.scrollTop = editor.scrollTop;
            code.parentElement.scrollLeft = editor.scrollLeft;
        };
        editor.oninput = () => {
            save(editor);
            syncCsharp();
        };
        if (restored) {
            editor.dispatchEvent(new Event('input', { bubbles: true }));
        }
    }

    function save(editor) {
        const key = editor.dataset.sessionKey;
        if (!key) {
            return;
        }

        try {
            localStorage.setItem(key, editor.value);
        } catch {
        }
    }

    function restoreEditor(editor) {
        const key = editor.dataset.sessionKey;
        if (!key) {
            return false;
        }

        try {
            const saved = localStorage.getItem(key);
            if (saved === null || saved === editor.value) {
                return false;
            }

            editor.value = saved;
            return true;
        } catch {
            return false;
        }
    }

    function syncSql() {
        const editor = document.getElementById('sql-editor');
        if (!editor) {
            return;
        }

        const restored = restoreEditor(editor);
        editor.oninput = () => save(editor);
        if (restored) {
            editor.dispatchEvent(new Event('input', { bubbles: true }));
        }
    }

    function syncAll() {
        syncCsharp();
        syncSql();
    }

    function restore(key, fallback) {
        try {
            return localStorage.getItem(key) ?? fallback;
        } catch {
            return fallback;
        }
    }

    function clear(key, fallback) {
        try {
            localStorage.removeItem(key);
            const editor = document.querySelector(`[data-session-key="${CSS.escape(key)}"]`);
            if (!editor) {
                return;
            }

            editor.value = fallback;
            editor.dispatchEvent(new Event('input', { bubbles: true }));
            localStorage.removeItem(key);
        } catch {
        }
    }

    function replaceSelection(editor, replacement, selectionStart, selectionEnd) {
        editor.setRangeText(replacement, editor.selectionStart, editor.selectionEnd, 'end');
        editor.setSelectionRange(selectionStart, selectionEnd);
        editor.dispatchEvent(new Event('input', { bubbles: true }));
    }

    function selectedLineRange(editor) {
        const start = editor.value.lastIndexOf('\n', editor.selectionStart - 1) + 1;
        let end = editor.value.indexOf('\n', editor.selectionEnd);
        if (end === -1) {
            end = editor.value.length;
        }
        return { start, end };
    }

    function indentSelection(editor, indent, outdent) {
        if (editor.selectionStart === editor.selectionEnd && !outdent) {
            const cursor = editor.selectionStart;
            replaceSelection(editor, indent, cursor + indent.length, cursor + indent.length);
            return;
        }

        const originalStart = editor.selectionStart;
        const originalEnd = editor.selectionEnd;
        const range = selectedLineRange(editor);
        const lines = editor.value.slice(range.start, range.end).split('\n');
        let removedBeforeStart = 0;
        let totalChange = 0;

        const replacement = lines.map((line, index) => {
            if (!outdent) {
                totalChange += indent.length;
                return indent + line;
            }

            const removable = line.startsWith('\t')
                ? 1
                : Math.min(indent.length, line.match(/^ */)[0].length);
            if (index === 0) {
                removedBeforeStart = Math.min(removable, originalStart - range.start);
            }
            totalChange -= removable;
            return line.slice(removable);
        }).join('\n');

        editor.setSelectionRange(range.start, range.end);
        const nextStart = outdent
            ? originalStart - removedBeforeStart
            : originalStart + indent.length;
        const nextEnd = Math.max(nextStart, originalEnd + totalChange);
        replaceSelection(editor, replacement, nextStart, nextEnd);
    }

    function insertNewLine(editor, indent) {
        const cursor = editor.selectionStart;
        const lineStart = editor.value.lastIndexOf('\n', cursor - 1) + 1;
        const currentLine = editor.value.slice(lineStart, cursor);
        const leadingWhitespace = currentLine.match(/^\s*/)[0];
        const extraIndent = /[{[(]\s*$/.test(currentLine) ? indent : '';
        const nextCharacter = editor.value[cursor];

        if (extraIndent && /[}\])]/.test(nextCharacter)) {
            const insertion = `\n${leadingWhitespace}${extraIndent}\n${leadingWhitespace}`;
            const nextCursor = cursor + 1 + leadingWhitespace.length + extraIndent.length;
            replaceSelection(editor, insertion, nextCursor, nextCursor);
            return;
        }

        const insertion = `\n${leadingWhitespace}${extraIndent}`;
        const nextCursor = cursor + insertion.length;
        replaceSelection(editor, insertion, nextCursor, nextCursor);
    }

    function toggleComments(editor, marker) {
        const originalStart = editor.selectionStart;
        const originalEnd = editor.selectionEnd;
        const range = selectedLineRange(editor);
        const lines = editor.value.slice(range.start, range.end).split('\n');
        const nonEmptyLines = lines.filter(line => line.trim().length > 0);
        const shouldUncomment = nonEmptyLines.length > 0 &&
            nonEmptyLines.every(line => line.trimStart().startsWith(marker));
        let totalChange = 0;

        const replacement = lines.map(line => {
            if (!line.trim()) {
                return line;
            }

            const whitespaceLength = line.length - line.trimStart().length;
            if (shouldUncomment) {
                const markerEnd = whitespaceLength + marker.length;
                const trailingSpace = line[markerEnd] === ' ' ? 1 : 0;
                totalChange -= marker.length + trailingSpace;
                return line.slice(0, whitespaceLength) + line.slice(markerEnd + trailingSpace);
            }

            totalChange += marker.length + 1;
            return `${line.slice(0, whitespaceLength)}${marker} ${line.slice(whitespaceLength)}`;
        }).join('\n');

        editor.setSelectionRange(range.start, range.end);
        replaceSelection(
            editor,
            replacement,
            originalStart,
            Math.max(originalStart, originalEnd + totalChange));
    }

    function wrapSelection(editor, opening, closing) {
        const start = editor.selectionStart;
        const end = editor.selectionEnd;
        if (start === end && editor.value[start] === closing) {
            editor.setSelectionRange(start + 1, start + 1);
            return;
        }

        const selectedText = editor.value.slice(start, end);
        replaceSelection(editor, opening + selectedText + closing, start + 1, end + 1);
    }

    function removeEmptyPair(editor) {
        const cursor = editor.selectionStart;
        if (cursor !== editor.selectionEnd || cursor === 0) {
            return false;
        }

        const pairs = { '(': ')', '[': ']', '{': '}', '"': '"', "'": "'" };
        if (pairs[editor.value[cursor - 1]] !== editor.value[cursor]) {
            return false;
        }

        editor.setSelectionRange(cursor - 1, cursor + 1);
        replaceSelection(editor, '', cursor - 1, cursor - 1);
        return true;
    }

    document.addEventListener('keydown', event => {
        const editor = event.target;
        const settings = editorSettings[editor.id];
        if (!settings) {
            return;
        }

        if (event.key === 'Tab') {
            event.preventDefault();
            indentSelection(editor, settings.indent, event.shiftKey);
        } else if (event.key === 'Enter' && !event.ctrlKey && !event.metaKey) {
            event.preventDefault();
            insertNewLine(editor, settings.indent);
        } else if ((event.ctrlKey || event.metaKey) && event.key === '/') {
            event.preventDefault();
            toggleComments(editor, settings.comment);
        } else if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
            event.preventDefault();
            document.querySelector('.toolbar .btn.primary:not(:disabled)')?.click();
        } else if (event.key === 'Backspace' && removeEmptyPair(editor)) {
            event.preventDefault();
        } else if (!event.ctrlKey && !event.metaKey && !event.altKey &&
            Object.hasOwn({ '(': ')', '[': ']', '{': '}', '"': '"', "'": "'" }, event.key)) {
            event.preventDefault();
            const pairs = { '(': ')', '[': ']', '{': '}', '"': '"', "'": "'" };
            wrapSelection(editor, event.key, pairs[event.key]);
        }
    });

    return { sync: syncCsharp, syncAll, restore, clear };
})();
