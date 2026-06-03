.386
.model flat,stdcall

EXTERN calcStatus:BYTE
EXTERN calcMem:DWORD
EXTERN runningTxt:BYTE
EXTERN terminatedTxt:BYTE

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
ReadConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
wsprintfA PROTO C :DWORD,:DWORD,:VARARG
lstrcpyA PROTO :DWORD,:DWORD

STD_OUTPUT_HANDLE EQU -11
STD_INPUT_HANDLE EQU -10

includelib kernel32.lib
includelib user32.lib

.data

calcTitle db 13,10,"=== Calculator Module ===",13,10,0

firstMsg db "First Number: ",0
opMsg db "Operator (+ - * /): ",0
secondMsg db "Second Number: ",0

resultFmt db 13,10,"Result = %d.%02d",13,10,0

num1 db 30 dup(0)
num2 db 30 dup(0)
op db 10 dup(0)

buffer db 100 dup(0)
bytesRead dd ?

n1 dd ?
n2 dd ?
answer dd ?

whole dd ?
frac dd ?

.code

; ==========================
; STRING → NUMBER (x100)
; ==========================

StrToInt PROC uses ebx ecx edx esi pStr:DWORD

    mov esi,pStr
    xor eax,eax
    xor ecx,ecx

    mov bl,[esi]

    cmp bl,'-'
    jne START

    mov ecx,1
    inc esi

START:

PARSE_INT:

    mov bl,[esi]

    cmp bl,'.'
    je PARSE_DECIMAL

    cmp bl,13
    je NO_DECIMAL

    cmp bl,10
    je NO_DECIMAL

    cmp bl,0
    je NO_DECIMAL

    sub bl,'0'

    imul eax,10
    movzx edx,bl
    add eax,edx

    inc esi
    jmp PARSE_INT

NO_DECIMAL:

    imul eax,100
    jmp APPLY_SIGN

PARSE_DECIMAL:

    imul eax,100

    inc esi

    mov bl,[esi]

    cmp bl,13
    je APPLY_SIGN

    sub bl,'0'

    movzx edx,bl
    imul edx,10
    add eax,edx

    inc esi

    mov bl,[esi]

    cmp bl,13
    je APPLY_SIGN

    sub bl,'0'

    movzx edx,bl
    add eax,edx

APPLY_SIGN:

    cmp ecx,1
    jne DONE

    neg eax

DONE:

    ret

StrToInt ENDP

; ==========================
; CALCULATOR
; ==========================

CalculatorSystem PROC PUBLIC

    invoke lstrcpyA,ADDR calcStatus,ADDR runningTxt

    mov calcMem,64

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke GetStdHandle,STD_INPUT_HANDLE
    mov esi,eax

    invoke WriteConsoleA,ebx,ADDR calcTitle,30,0,0

    invoke WriteConsoleA,ebx,ADDR firstMsg,14,0,0
    invoke ReadConsoleA,esi,ADDR num1,30,ADDR bytesRead,0

    invoke WriteConsoleA,ebx,ADDR opMsg,20,0,0
    invoke ReadConsoleA,esi,ADDR op,10,ADDR bytesRead,0

    invoke WriteConsoleA,ebx,ADDR secondMsg,15,0,0
    invoke ReadConsoleA,esi,ADDR num2,30,ADDR bytesRead,0

    invoke StrToInt,ADDR num1
    mov n1,eax

    invoke StrToInt,ADDR num2
    mov n2,eax

    mov al,[op]

    cmp al,'+'
    je DO_ADD

    cmp al,'-'
    je DO_SUB

    cmp al,'*'
    je DO_MUL

    cmp al,'/'
    je DO_DIV

    jmp FINISH

DO_ADD:

    mov eax,n1
    add eax,n2
    mov answer,eax
    jmp SHOW

DO_SUB:

    mov eax,n1
    sub eax,n2
    mov answer,eax
    jmp SHOW

DO_MUL:

    mov eax,n1
    imul n2

    cdq
    mov ecx,100
    idiv ecx

    mov answer,eax
    jmp SHOW

DO_DIV:

    mov eax,n1
    imul eax,100

    cdq
    idiv n2

    mov answer,eax
    jmp SHOW

SHOW:

    mov eax,answer

    cdq
    mov ecx,100
    idiv ecx

    mov whole,eax

    mov eax,edx

    cmp eax,0
    jge POSITIVE

    neg eax

POSITIVE:

    mov frac,eax

    invoke wsprintfA,\
        ADDR buffer,\
        ADDR resultFmt,\
        whole,\
        frac

    invoke WriteConsoleA,\
        ebx,\
        ADDR buffer,\
        100,\
        0,\
        0

FINISH:

    invoke lstrcpyA,\
        ADDR calcStatus,\
        ADDR terminatedTxt

    ret

CalculatorSystem ENDP

END