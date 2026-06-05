.386
.model flat,stdcall

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
ReadConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
lstrcmpA PROTO :DWORD,:DWORD

STD_OUTPUT_HANDLE EQU -11
STD_INPUT_HANDLE  EQU -10

includelib kernel32.lib
includelib user32.lib

.data

privatePrompt db 13,10,"🔒 PRIVATE VAULT ACCESS",13,10
db "Enter Password: ",0
pPromptLen EQU ($-privatePrompt)

accessGranted db 13,10,"Access Granted! Opening Private Vault...",13,10,0
grantLen EQU ($-accessGranted)

accessDenied db 13,10,"Access Denied! Invalid Password.",13,10,0
deniedLen EQU ($-accessDenied)

correctPass db "admin123",0

passInput db 50 dup(0)
bytesRead dd ?

.code

LoginSystem PROC PUBLIC

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke GetStdHandle,STD_INPUT_HANDLE
    mov esi,eax

    invoke WriteConsoleA,ebx,ADDR privatePrompt,pPromptLen,0,0

    invoke ReadConsoleA,esi,ADDR passInput,50,ADDR bytesRead,0

    mov eax,bytesRead
    sub eax,2
    mov byte ptr [passInput+eax],0

    invoke lstrcmpA,ADDR passInput,ADDR correctPass
    cmp eax,0
    jne ACCESS_DENIED

ACCESS_GRANTED:
    invoke WriteConsoleA,ebx,ADDR accessGranted,grantLen,0,0
    mov eax,1  ; Return 1 for success
    ret

ACCESS_DENIED:
    invoke WriteConsoleA,ebx,ADDR accessDenied,deniedLen,0,0
    xor eax,eax  ; Return 0 for failure
    ret

LoginSystem ENDP

END