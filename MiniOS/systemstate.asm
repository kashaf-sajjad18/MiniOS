.386
.model flat,stdcall

.data

PUBLIC calcStatus
PUBLIC timeStatus
PUBLIC waitingTxt
PUBLIC runningTxt
PUBLIC terminatedTxt

PUBLIC kernelMem
PUBLIC shellMem
PUBLIC calcMem
PUBLIC timeMem
PUBLIC taskMem

PUBLIC totalRAM
PUBLIC usedRAM
PUBLIC freeRAM


calcStatus db "WAITING",0
timeStatus db "WAITING",0

waitingTxt db "WAITING",0
runningTxt db "RUNNING",0
terminatedTxt db "TERMINATED",0

kernelMem dd 256
shellMem dd 128
calcMem dd 64
timeMem dd 32
taskMem dd 64

totalRAM dd 4096
usedRAM dd 480
freeRAM dd 3616

.code
END