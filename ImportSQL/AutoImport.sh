echo "Attempting to Import Sql Scripts"
while /bin/sleep 1; 
do
    /opt/mssql-tools/bin/sqlcmd -S mssql-server,1433 -U sa -P DingoChat5% -d master -i "./CheckStatus.sql"
    if [ $? -eq 0 ]
    then
        for entry in "/var/opt/mssql/import/SqlToImport"/*
        do
            /opt/mssql-tools/bin/sqlcmd -S mssql-server,1433 -U sa -P DingoChat5% -d master -i $entry
            echo "Imported: "$entry
        done
        echo "Finished Importing sql scripts"
        break
    else
        echo " "
        echo "Error code: 0x68 is NORMAL (failed to log in, sql not ready)"
        echo "Waiting on MSSQL to start before importing sql"
        echo " "
    fi
done