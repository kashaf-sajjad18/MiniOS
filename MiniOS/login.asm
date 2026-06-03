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

usernamePrompt db 13,10,"Username: "
uLen EQU ($-usernamePrompt)

passwordPrompt db 13,10,"Password: "
pLen EQU ($-passwordPrompt)

successMsg db 13,10,"Login Successful!",13,10
sLen EQU ($-successMsg)

failMsg db 13,10,"Invalid Username or Password!",13,10
fLen EQU ($-failMsg)

correctUser db "admin",13,10,0
correctPass db "1234",13,10,0

userInput db 50 dup(0)
passInput db 50 dup(0)

bytesRead dd ?

.code

LoginSystem PROC PUBLIC

    ; output handle
    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    ; input handle
    invoke GetStdHandle,STD_INPUT_HANDLE
    mov esi,eax

    ; USERNAME
    invoke WriteConsoleA,ebx,ADDR usernamePrompt,uLen,0,0
    invoke ReadConsoleA,esi,ADDR userInput,50,ADDR bytesRead,0

    ; PASSWORD
    invoke WriteConsoleA,ebx,ADDR passwordPrompt,pLen,0,0
    invoke ReadConsoleA,esi,ADDR passInput,50,ADDR bytesRead,0

    ; username check
    invoke lstrcmpA,ADDR userInput,ADDR correctUser
    cmp eax,0
    jne LOGIN_FAIL

    ; password check
    invoke lstrcmpA,ADDR passInput,ADDR correctPass
    cmp eax,0
    jne LOGIN_FAIL

LOGIN_OK:
    invoke WriteConsoleA,ebx,ADDR successMsg,sLen,0,0
    ret

LOGIN_FAIL:
    invoke WriteConsoleA,ebx,ADDR failMsg,fLen,0,0
    ret

LoginSystem ENDP

END