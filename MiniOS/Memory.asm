.386
.model flat,stdcall

EXTERN calcMem:DWORD
EXTERN timeMem:DWORD

EXTERN totalRAM:DWORD
EXTERN kernelMem:DWORD
EXTERN shellMem:DWORD
EXTERN taskMem:DWORD

EXTERN usedRAM:DWORD
EXTERN freeRAM:DWORD

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
wsprintfA PROTO C :DWORD,:DWORD,:VARARG

STD_OUTPUT_HANDLE EQU -11

includelib kernel32.lib
includelib user32.lib

.data

memTitle db 13,10,"===== MEMORY MANAGER =====",13,10,13,10,0

fmt1 db "TOTAL RAM      : %d KB",13,10,0
fmt2 db "USED RAM       : %d KB",13,10,0
fmt3 db "FREE RAM       : %d KB",13,10,13,10,0

kernelFmt db "Kernel         : %d KB",13,10,0
shellFmt  db "Shell          : %d KB",13,10,0
taskFmt   db "Task Manager   : %d KB",13,10,0
calcFmt   db "Calculator     : %d KB",13,10,0
timeFmt   db "Time Service   : %d KB",13,10,0

buffer db 200 dup(0)

.code

MemorySystem PROC PUBLIC

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    mov eax,kernelMem
    add eax,shellMem
    add eax,taskMem
    add eax,calcMem
    add eax,timeMem

    mov usedRAM,eax

    mov eax,totalRAM
    sub eax,usedRAM

    mov freeRAM,eax

    invoke WriteConsoleA,ebx,ADDR memTitle,50,0,0

    invoke wsprintfA,ADDR buffer,ADDR fmt1,totalRAM
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR fmt2,usedRAM
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR fmt3,freeRAM
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR kernelFmt,kernelMem
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR shellFmt,shellMem
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR taskFmt,taskMem
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR calcFmt,calcMem
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    invoke wsprintfA,ADDR buffer,ADDR timeFmt,timeMem
    invoke WriteConsoleA,ebx,ADDR buffer,200,0,0

    ret

MemorySystem ENDP

END