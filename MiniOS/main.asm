; ============================================================
; main.asm  -  NOVA-OS Entry Point
;
; GUI calls:  Backend.exe boot
;             Backend.exe time
;             Backend.exe calc
;             Backend.exe mem
;             Backend.exe task
;             Backend.exe explorer create <folder> <filename> <content>
;             Backend.exe explorer delete <folder> <filename>
;             Backend.exe explorer list   <folder>
;             Backend.exe explorer mkdir  <folder> <name>
;             Backend.exe explorer checkpass <password>
;
; No argument -> full Boot + Shell (interactive console mode)
; ============================================================

.386
.model flat, stdcall
.stack 4096

ExitProcess        PROTO :DWORD
BootSystem         PROTO
ShellSystem        PROTO
TimeSystem         PROTO
CalculatorSystem   PROTO
MemorySystem       PROTO
TaskSystem         PROTO
FileExplorerSystem PROTO

GetCommandLineA    PROTO
lstrcmpA           PROTO :DWORD, :DWORD
lstrcpyA           PROTO :DWORD, :DWORD
lstrlenA           PROTO :DWORD
GetStdHandle       PROTO :DWORD
WriteConsoleA      PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD

CreateDirectoryA   PROTO :DWORD,:DWORD
CreateFileA        PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
WriteFile          PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
CloseHandle        PROTO :DWORD
DeleteFileA        PROTO :DWORD
FindFirstFileA     PROTO :DWORD,:DWORD
FindNextFileA      PROTO :DWORD,:DWORD
FindClose          PROTO :DWORD

wsprintfA          PROTO C :DWORD,:DWORD,:VARARG

includelib kernel32.lib
includelib user32.lib

STD_OUTPUT_HANDLE  EQU -11
GENERIC_WRITE      EQU 40000000h
OPEN_ALWAYS        EQU 4
FILE_ATTRIBUTE_NORMAL EQU 80h
INVALID_HANDLE_VALUE  EQU -1

; ============================================================
.data
; ============================================================

; --- command tokens ---
tokBoot      db "boot",0
tokTime      db "time",0
tokCalc      db "calc",0
tokMem       db "mem",0
tokTask      db "task",0
tokExplorer  db "explorer",0

; --- explorer sub-command tokens ---
tokCreate    db "create",0
tokDelete    db "delete",0
tokList      db "list",0
tokMkdir     db "mkdir",0
tokCheckPass db "checkpass",0

; --- password ---
correctPass  db "admin123",0

; --- messages ---
msgOK        db "[OK]",13,10,0
msgFail      db "[FAIL]",13,10,0
msgGranted   db "[ACCESS GRANTED]",13,10,0
msgDenied    db "[ACCESS DENIED]",13,10,0
msgSep       db "---",13,10,0
msgNewline   db 13,10,0

; --- file listing format ---
fmtFile      db "%s",13,10,0

; --- WIN32_FIND_DATA buffer (320 bytes) ---
; offset 0 = dwFileAttributes (DWORD)
; offset 44 = cFileName (260 bytes)
findData     db 320 dup(0)

; --- scratch buffers ---
pathBuf      db 520 dup(0)   ; full path = folder + "\" + filename
outBuf       db 300 dup(0)

bytesWritten dd 0
hFile        dd 0
hFind        dd 0

; pointer to current arg position in command line
pCur         dd 0

; ============================================================
.code
; ============================================================

; ------------------------------------------------------------
; PrintStr - write NUL-terminated string to stdout
; ------------------------------------------------------------
PrintStr PROC uses ebx pStr:DWORD
    invoke GetStdHandle, STD_OUTPUT_HANDLE
    mov    ebx, eax
    invoke lstrlenA, pStr
    invoke WriteConsoleA, ebx, pStr, eax, 0, 0
    ret
PrintStr ENDP

; ------------------------------------------------------------
; BuildPath  -  pathBuf = folder + "\" + filename
;   pFolder -> EAX arg 1
;   pName   -> EAX arg 2  (pass on stack yourself via inline)
; We use a simple inline helper below instead.
; ------------------------------------------------------------

; ------------------------------------------------------------
; SkipToArg  - advance past exe name, return ptr to remainder
;              (or 0 if nothing follows)
; ------------------------------------------------------------
SkipToArg PROC uses esi
    invoke GetCommandLineA
    mov    esi, eax

skip_lead:
    mov    al, [esi]
    cmp    al, ' '
    jne    chk_quote
    inc    esi
    jmp    skip_lead

chk_quote:
    cmp    al, '"'
    je     in_quote

unquoted:
    mov    al, [esi]
    cmp    al, 0
    je     none
    cmp    al, ' '
    je     skip_sp
    inc    esi
    jmp    unquoted

in_quote:
    inc    esi
eat_quote:
    mov    al, [esi]
    cmp    al, 0
    je     none
    inc    esi
    cmp    al, '"'
    jne    eat_quote

skip_sp:
    mov    al, [esi]
    cmp    al, 0
    je     none
    cmp    al, ' '
    jne    got_it
    inc    esi
    jmp    skip_sp

got_it:
    mov    eax, esi
    ret
none:
    xor    eax, eax
    ret
SkipToArg ENDP

; ------------------------------------------------------------
; NextToken  - given pointer in pCur, copy next
;              whitespace-delimited token into destBuf,
;              advance pCur past it.  Returns 0 if no token.
; ------------------------------------------------------------
NextToken PROC uses esi edi pDest:DWORD
    mov    esi, pCur
    ; skip leading spaces
skip_sp2:
    mov    al, [esi]
    cmp    al, ' '
    jne    copy_tok
    inc    esi
    jmp    skip_sp2

copy_tok:
    cmp    byte ptr [esi], 0
    je     no_tok
    ; esi points at start of token - copy until space/NUL
    mov    edi, pDest
copy_loop:
    mov    al, [esi]
    cmp    al, 0
    je     done_tok
    cmp    al, ' '
    je     done_tok
    mov    [edi], al
    inc    esi
    inc    edi
    jmp    copy_loop
done_tok:
    mov    byte ptr [edi], 0   ; NUL terminate
    mov    pCur, esi
    mov    eax, 1
    ret
no_tok:
    mov    pCur, esi
    xor    eax, eax
    ret
NextToken ENDP

; ------------------------------------------------------------
; CopyRestOfLine - copy everything from pCur to NUL into dest
; ------------------------------------------------------------
CopyRestOfLine PROC uses esi edi pDest:DWORD
    mov    esi, pCur
    ; skip leading spaces
skip3:
    mov    al, [esi]
    cmp    al, ' '
    jne    copy3
    inc    esi
    jmp    skip3
copy3:
    mov    edi, pDest
copy3loop:
    mov    al, [esi]
    mov    [edi], al
    cmp    al, 0
    je     done3
    inc    esi
    inc    edi
    jmp    copy3loop
done3:
    ret
CopyRestOfLine ENDP

; ------------------------------------------------------------
; HandleExplorer  - parse sub-command and act
;
; Syntax coming from C# ProcessBuilder args:
;   explorer create <folder> <filename> <content...>
;   explorer delete <folder> <filename>
;   explorer list   <folder>
;   explorer mkdir  <folder> <foldername>
;   explorer checkpass <password>
; ------------------------------------------------------------

; local token buffers (file-scope, 260 bytes each)
subCmd   db 64  dup(0)
argFoldr db 260 dup(0)
argName  db 260 dup(0)
argCont  db 4096 dup(0)

HandleExplorer PROC

    ; --- read sub-command ---
    invoke NextToken, ADDR subCmd
    test   eax, eax
    jz     he_done

    ; --- CREATE ---
    invoke lstrcmpA, ADDR subCmd, ADDR tokCreate
    test   eax, eax
    jz     he_create

    ; --- DELETE ---
    invoke lstrcmpA, ADDR subCmd, ADDR tokDelete
    test   eax, eax
    jz     he_delete

    ; --- LIST ---
    invoke lstrcmpA, ADDR subCmd, ADDR tokList
    test   eax, eax
    jz     he_list

    ; --- MKDIR ---
    invoke lstrcmpA, ADDR subCmd, ADDR tokMkdir
    test   eax, eax
    jz     he_mkdir

    ; --- CHECKPASS ---
    invoke lstrcmpA, ADDR subCmd, ADDR tokCheckPass
    test   eax, eax
    jz     he_checkpass

    jmp    he_done

; ---- CREATE ----
he_create:
    invoke NextToken, ADDR argFoldr   ; folder
    invoke NextToken, ADDR argName    ; filename
    invoke CopyRestOfLine, ADDR argCont ; rest = content

    ; build path: pathBuf = argFoldr + "\" + argName
    invoke lstrcpyA, ADDR pathBuf, ADDR argFoldr
    invoke lstrlenA, ADDR pathBuf
    mov    edi, eax
    mov    byte ptr [pathBuf + edi], '\'
    inc    edi
    ; append argName
    mov    esi, OFFSET argName
app_name:
    mov    al, [esi]
    mov    [pathBuf + edi], al
    cmp    al, 0
    je     name_done
    inc    esi
    inc    edi
    jmp    app_name
name_done:

    ; CreateFile
    invoke CreateFileA, ADDR pathBuf, GENERIC_WRITE, 0, 0, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0
    cmp    eax, INVALID_HANDLE_VALUE
    je     he_create_fail

    mov    hFile, eax
    invoke lstrlenA, ADDR argCont
    invoke WriteFile, hFile, ADDR argCont, eax, ADDR bytesWritten, 0
    invoke CloseHandle, hFile
    invoke PrintStr, ADDR msgOK
    jmp    he_done

he_create_fail:
    invoke PrintStr, ADDR msgFail
    jmp    he_done

; ---- DELETE ----
he_delete:
    invoke NextToken, ADDR argFoldr
    invoke NextToken, ADDR argName

    invoke lstrcpyA, ADDR pathBuf, ADDR argFoldr
    invoke lstrlenA, ADDR pathBuf
    mov    edi, eax
    mov    byte ptr [pathBuf + edi], '\'
    inc    edi
    mov    esi, OFFSET argName
app_name2:
    mov    al, [esi]
    mov    [pathBuf + edi], al
    cmp    al, 0
    je     n2done
    inc    esi
    inc    edi
    jmp    app_name2
n2done:
    invoke DeleteFileA, ADDR pathBuf
    cmp    eax, 0
    je     he_del_fail
    invoke PrintStr, ADDR msgOK
    jmp    he_done
he_del_fail:
    invoke PrintStr, ADDR msgFail
    jmp    he_done

; ---- LIST ----
he_list:
    invoke NextToken, ADDR argFoldr

    ; pattern = argFoldr + "\*"
    invoke lstrcpyA, ADDR pathBuf, ADDR argFoldr
    invoke lstrlenA, ADDR pathBuf
    mov    edi, eax
    mov    byte ptr [pathBuf + edi],   '\'
    mov    byte ptr [pathBuf + edi+1], '*'
    mov    byte ptr [pathBuf + edi+2], 0

    invoke FindFirstFileA, ADDR pathBuf, ADDR findData
    cmp    eax, INVALID_HANDLE_VALUE
    je     he_done

    mov    hFind, eax

list_loop:
    ; cFileName is at offset 44 in WIN32_FIND_DATA
    lea    esi, [findData + 44]

    ; skip "." and ".."
    mov    al, [esi]
    cmp    al, '.'
    je     list_next

    invoke wsprintfA, ADDR outBuf, ADDR fmtFile, esi
    invoke PrintStr, ADDR outBuf

list_next:
    invoke FindNextFileA, hFind, ADDR findData
    test   eax, eax
    jnz    list_loop

    invoke FindClose, hFind
    jmp    he_done

; ---- MKDIR ----
he_mkdir:
    invoke NextToken, ADDR argFoldr
    invoke NextToken, ADDR argName

    invoke lstrcpyA, ADDR pathBuf, ADDR argFoldr
    invoke lstrlenA, ADDR pathBuf
    mov    edi, eax
    mov    byte ptr [pathBuf + edi], '\'
    inc    edi
    mov    esi, OFFSET argName
app_name3:
    mov    al, [esi]
    mov    [pathBuf + edi], al
    cmp    al, 0
    je     n3done
    inc    esi
    inc    edi
    jmp    app_name3
n3done:
    invoke CreateDirectoryA, ADDR pathBuf, 0
    test   eax, eax
    jz     he_mkdir_fail
    invoke PrintStr, ADDR msgOK
    jmp    he_done
he_mkdir_fail:
    invoke PrintStr, ADDR msgFail
    jmp    he_done

; ---- CHECKPASS ----
he_checkpass:
    invoke NextToken, ADDR argName    ; argName holds the attempt
    invoke lstrcmpA, ADDR argName, ADDR correctPass
    test   eax, eax
    jz     he_pass_ok
    invoke PrintStr, ADDR msgDenied
    jmp    he_done
he_pass_ok:
    invoke PrintStr, ADDR msgGranted

he_done:
    ret
HandleExplorer ENDP

; ------------------------------------------------------------
; main
; ------------------------------------------------------------

tok1   db 64 dup(0)   ; first argument token

main PROC

    call   SkipToArg
    test   eax, eax
    jz     run_full        ; no argument at all

    ; store pointer so NextToken can use it
    mov    pCur, eax

    ; read first token into tok1
    invoke NextToken, ADDR tok1

    invoke lstrcmpA, ADDR tok1, ADDR tokBoot
    test   eax, eax
    jz     do_boot

    invoke lstrcmpA, ADDR tok1, ADDR tokTime
    test   eax, eax
    jz     do_time

    invoke lstrcmpA, ADDR tok1, ADDR tokCalc
    test   eax, eax
    jz     do_calc

    invoke lstrcmpA, ADDR tok1, ADDR tokMem
    test   eax, eax
    jz     do_mem

    invoke lstrcmpA, ADDR tok1, ADDR tokTask
    test   eax, eax
    jz     do_task

    invoke lstrcmpA, ADDR tok1, ADDR tokExplorer
    test   eax, eax
    jz     do_explorer

    jmp    run_full    ; unknown arg -> full mode

do_boot:
    call   BootSystem
    invoke ExitProcess, 0

do_time:
    call   TimeSystem
    invoke ExitProcess, 0

do_calc:
    call   CalculatorSystem
    invoke ExitProcess, 0

do_mem:
    call   MemorySystem
    invoke ExitProcess, 0

do_task:
    call   TaskSystem
    invoke ExitProcess, 0

do_explorer:
    call   HandleExplorer
    invoke ExitProcess, 0

run_full:
    call   BootSystem
    call   ShellSystem
    invoke ExitProcess, 0

main ENDP

END main