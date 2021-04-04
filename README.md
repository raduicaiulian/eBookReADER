# eBookReADER
 Is an app that read tor b electronic books, the formats accepted are:

      - pdf
      - docx
      - txt(plain text)

In order to convert the docx/pdf to txt files I used a custom TCP server writen in python named **convert_server.py**, în c# I send the file to server to get the  plain text from input file.
After file conversion is complete the text is sent to Google Text-to-Speech API in order to obtain am .mp3 file coresponding to previous text file, and the last step is do play the audio fkile to user.
The GUI is made using WinForms

Os interaction si made trough socket creation used to talk to convert server
TO DO:

  - add more error handling;
  - add save audio books to internal storage feature;
  - optimize both convert server and client;

Optional tasks:

  -display mp3 playesr status;
  -add suport for both EN and RO languages(curently only RO is suported);  
  -give user posibilyty to skip to a specific point into audio file;
  
