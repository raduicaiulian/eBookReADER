#!/bin/python3
import pdftotext
import docx2txt
import socket
import sys
from struct import *
#--------------------------------------------------set up the serrver
TCP_IP = '0.0.0.0'
TCP_PORT = 9999
BUFFER_SIZE = 4

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((TCP_IP, TCP_PORT))
s.listen(100)
for i in range(100):
    print("Waiting for conections")

    conn, addr = s.accept()
    print('Connection address:', addr)
    data_size=b''
    while(len(data_size)!=BUFFER_SIZE):
        data_size = conn.recv(BUFFER_SIZE)
    #decode
    data_size = int.from_bytes(data_size, byteorder='little') 
    file_data=b''
    while(data_size!=len(file_data)):
        file_data += conn.recv(data_size)
        #file_data = file_data.decode('utf-8')
        print("data size:", data_size)
        print("sctual data length:", len(file_data))
    print("sctual data length:", len(file_data))
    #write received bytes to a file

    if (file_data[1:4]==b'PDF'):
       is_pdf=1
       print("am primit un pdf")
       f = open('file.pdf', 'wb')
    else:
        is_pdf=0
        print("am primit un docx")
        f = open('file.docx', 'wb')
    f.write(file_data)
    f.close()
    #convert received file
    if is_pdf==1:
        with open("file.pdf", "rb") as f:
            pdf = pdftotext.PDF(f)
            i=1
            text=''
            for page in pdf:
                text+= "Pagina "+str(i)+":\n"
                text+= page
                i+=1
    else:
        #TO DO: add page number for docx
        text = docx2txt.process("file.docx")
        print("text from word:")
        print(text)

    #send back the response
    text = text.encode("utf-8")
    text_len =  pack('I',len(text))
    print("lungimea la datele din fișierul txt: "+str(text_len)+" de size: "+str(len(text_len)))
    conn.sendall(text_len)
    conn.sendall(text)

    conn.close()
    print("CLIENT SERVIT CU SUCCES!")











#--------------------------------------------------pdf convert
"""file="aaa.pdf"
#file="aaa.docx"
# Load your PDF
with open(file, "rb") as f:
    pdf = pdftotext.PDF(f)

# If it's password-protected
#with open("secure.pdf", "rb") as f:
#    pdf = pdftotext.PDF(f, "secret")

# How many pages?
#print(len(pdf))

# Iterate over all the pages
i=1
for page in pdf:
    print(f"Page {i}:")
    print(page)
    i+=1

# Read some individual pages
#print(pdf[0])
#print(pdf[1])

# Read all the text into one string
#print("\n\n".join(pdf))

#--------------------------------------------docx convert
text = docx2txt.process("aaa.docx")
print(text)

f = open("out.txt", "w")
f.write(text)
f.close()
#https://www.youtube.com/watch?v=V8LQ9lac2o8
"""
