.386
.model flat,stdcall

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
Sleep PROTO :DWORD

STD_OUTPUT_HANDLE EQU -11

includelib kernel32.lib
includelib user32.lib

.data

boot1 db 13,10,"Initializing Kernel............. [OK]",13,10,0
boot2 db "Loading Memory Manager.......... [OK]",13,10,0
boot3 db "Starting Task Service........... [OK]",13,10,0
boot4 db "Mounting File System............ [OK]",13,10,0
boot5 db "Loading Shell................... [OK]",13,10,13,10,0

readyMsg db "========== MINIOS READY ==========",13,10,13,10,0

.code

BootSystem PROC PUBLIC

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke WriteConsoleA,ebx,ADDR boot1,45,0,0
    invoke Sleep,300

    invoke WriteConsoleA,ebx,ADDR boot2,45,0,0
    invoke Sleep,300

    invoke WriteConsoleA,ebx,ADDR boot3,45,0,0
    invoke Sleep,300

    invoke WriteConsoleA,ebx,ADDR boot4,45,0,0
    invoke Sleep,300

    invoke WriteConsoleA,ebx,ADDR boot5,45,0,0
    invoke Sleep,300

    invoke WriteConsoleA,ebx,ADDR readyMsg,40,0,0

    ret

BootSystem ENDP

END