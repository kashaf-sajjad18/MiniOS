.386
.model flat,stdcall

GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
GetLocalTime PROTO :DWORD
wsprintfA PROTO C :DWORD,:DWORD,:VARARG

STD_OUTPUT_HANDLE EQU -11

includelib kernel32.lib
includelib user32.lib

SYSTEMTIME STRUCT
wYear WORD ?
wMonth WORD ?
wDayOfWeek WORD ?
wDay WORD ?
wHour WORD ?
wMinute WORD ?
wSecond WORD ?
wMilliseconds WORD ?
SYSTEMTIME ENDS

.data

timeTitle db 13,10,"=== Time Module ===",13,10,0
titleLen EQU ($-timeTitle)

fmt db "Current Time: %02d:%02d:%02d",13,10,0

sysTime SYSTEMTIME <>
buffer db 100 dup(0)

.code

TimeSystem PROC PUBLIC

    invoke GetStdHandle,STD_OUTPUT_HANDLE
    mov ebx,eax

    invoke GetLocalTime,ADDR sysTime

    invoke wsprintfA,ADDR buffer,ADDR fmt,sysTime.wHour,sysTime.wMinute,sysTime.wSecond

    invoke WriteConsoleA,ebx,ADDR timeTitle,titleLen,0,0

    invoke WriteConsoleA,ebx,ADDR buffer,100,0,0

    ret

TimeSystem ENDP

END