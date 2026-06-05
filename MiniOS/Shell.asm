.386
.model flat,stdcall

EXTERN calcStatus:BYTE
EXTERN timeStatus:BYTE
EXTERN waitingTxt:BYTE

TimeSystem PROTO
CalculatorSystem PROTO
MemorySystem PROTO
TaskSystem PROTO

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
ReadConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
lstrcmpA PROTO :DWORD,:DWORD
lstrcpyA PROTO :DWORD,:DWORD

STD_OUTPUT_HANDLE EQU -11
STD_INPUT_HANDLE EQU -10

includelib kernel32.lib
includelib user32.lib

.data

prompt db 13,10,"MiniOS> ",0
promptLen EQU ($-prompt)

helpCmd db "help",0
timeCmd db "time",0
calcCmd db "calc",0
memCmd db "mem",0
taskCmd db "task",0
shutdownCmd db "shutdown",0

welcomeMsg db 13,10
db "========= MINIOS TERMINAL =========",13,10
db "Available Commands:",13,10
db "help - show commands",13,10
db "time - show time",13,10
db "calc - calculator",13,10
db "mem  - memory status",13,10
db "task - task manager",13,10
db "shutdown - exit",13,10,13,10,0

welcomeLen EQU ($-welcomeMsg)

invalidMsg db 13,10,"Unknown Command!",13,10,0
invalidLen EQU ($-invalidMsg)

shutdownMsg db 13,10,"System Shutting Down...",13,10,0
shutdownLen EQU ($-shutdownMsg)

cmdInput db 50 dup(0)
bytesRead dd ?

.code

ShellSystem PROC PUBLIC

    invoke lstrcpyA,ADDR calcStatus,ADDR waitingTxt
    invoke lstrcpyA,ADDR timeStatus,ADDR waitingTxt

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke WriteConsoleA,ebx,ADDR welcomeMsg,welcomeLen,0,0

START_LOOP:

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke GetStdHandle,STD_INPUT_HANDLE
    mov esi,eax

    invoke WriteConsoleA,ebx,ADDR prompt,promptLen,0,0

    invoke ReadConsoleA,esi,ADDR cmdInput,50,ADDR bytesRead,0

    mov eax,bytesRead
    cmp eax,2
    jle START_LOOP

    sub eax,2
    mov byte ptr [cmdInput+eax],0

    invoke lstrcmpA,ADDR cmdInput,ADDR helpCmd
    cmp eax,0
    je SHOW_HELP

    invoke lstrcmpA,ADDR cmdInput,ADDR timeCmd
    cmp eax,0
    je SHOW_TIME

    invoke lstrcmpA,ADDR cmdInput,ADDR calcCmd
    cmp eax,0
    je SHOW_CALC

    invoke lstrcmpA,ADDR cmdInput,ADDR memCmd
    cmp eax,0
    je SHOW_MEM

    invoke lstrcmpA,ADDR cmdInput,ADDR taskCmd
    cmp eax,0
    je SHOW_TASK

    invoke lstrcmpA,ADDR cmdInput,ADDR shutdownCmd
    cmp eax,0
    je SHUTDOWN

    invoke WriteConsoleA,ebx,ADDR invalidMsg,invalidLen,0,0
    jmp START_LOOP

SHOW_HELP:
    invoke WriteConsoleA,ebx,ADDR welcomeMsg,welcomeLen,0,0
    jmp START_LOOP

SHOW_TIME:
    call TimeSystem
    jmp START_LOOP

SHOW_CALC:
    call CalculatorSystem
    jmp START_LOOP

SHOW_MEM:
    call MemorySystem
    jmp START_LOOP

SHOW_TASK:
    call TaskSystem
    jmp START_LOOP

SHUTDOWN:
    invoke WriteConsoleA,ebx,ADDR shutdownMsg,shutdownLen,0,0
    ret

ShellSystem ENDP
END