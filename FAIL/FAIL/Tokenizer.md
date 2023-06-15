# Foreach Token

## If Position Skipped
skip

## Set Metadata
- advance column
- set current position
- skip

## If End Of Line
- set column
- advance row
- skip

### If Line Comment							
end comment

## Check Comment State

### If Block Comment & Comment End
- skip
- skip next token
- reset comment state

### If in Comment
skip

### If Slash

#### If Line Comment Delimiter
- set comment state to line

#### If Block Comment Delimiter
- set comment state to block

#### If In Comment
skip

## If In Int

### If Digit
- add to list
- skip

### If Dot
- add to list
- change to double
- skip 

### Else
- check for valid token
- no longer in int
- skip

## If In Text (Variable, Function Name, ...)
- add to list
- skip

## If In String
- add to list

### If New Line
- set column
- advance row
- skip

### If End Of String
- check for valid token
- no longer in string
- skip

### Else
skip

## If In Double

### If Digit
- add to list
- skip

### Else
- check for valid token
- no longer in double
- skip

## If Char Detected
- check for full char
- add all to list
- now in char

### If Char Not Ended
- report error > no full char

### Else
- check for valid token
- skip to after end of char
- no longer in char
- skip

## If Whitespace
- check for valid token
- skip

## If Operator
- check for valid token
- add operator
- skip

## If Boolean
- add Boolean
- skip to after Boolean
- empty buffer
- add token
- skip

## Else
- add to list

### If String Delimiter
- set state to string

### If Digit
- set state to int
