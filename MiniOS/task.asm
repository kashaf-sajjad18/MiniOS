.386
.model flat,stdcall

EXTERN calcStatus:BYTE
EXTERN timeStatus:BYTE

EXTERN calcMem:DWORD
EXTERN timeMem:DWORD

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
wsprintfA PROTO C :DWORD,:DWORD,:VARARG
lstrlenA PROTO :DWORD

STD_OUTPUT_HANDLE EQU -11

includelib kernel32.lib
includelib user32.lib

.data

taskTitle db 13,10,"===== TASK MANAGER =====",13,10,13,10,0
titleLen EQU ($-taskTitle)

header db "PID   PROCESS         STATUS        MEMORY",13,10,13,10,0
headerLen EQU ($-header)

kernelFmt db "001   Kernel          RUNNING       256 KB",13,10,0
kernelLen EQU ($-kernelFmt)

shellFmt db "002   Shell           RUNNING       128 KB",13,10,0
shellLen EQU ($-shellFmt)

calcFmt db "003   Calculator      %-10s   %d KB",13,10,0
timeFmt db "004   TimeService     %-10s   %d KB",13,10,0

buffer db 200 dup(0)

.code

TaskSystem PROC PUBLIC

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke WriteConsoleA,ebx,ADDR taskTitle,titleLen,0,0

    invoke WriteConsoleA,ebx,ADDR header,headerLen,0,0

    invoke WriteConsoleA,ebx,ADDR kernelFmt,kernelLen,0,0

    invoke WriteConsoleA,ebx,ADDR shellFmt,shellLen,0,0

    invoke wsprintfA,ADDR buffer,ADDR calcFmt,ADDR calcStatus,calcMem

    invoke lstrlenA,ADDR buffer

    invoke WriteConsoleA,ebx,ADDR buffer,eax,0,0

    invoke wsprintfA,ADDR buffer,ADDR timeFmt,ADDR timeStatus,timeMem

    invoke lstrlenA,ADDR buffer

    invoke WriteConsoleA,ebx,ADDR buffer,eax,0,0

    ret

TaskSystem ENDP

END