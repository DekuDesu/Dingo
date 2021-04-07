echo "Attempting to Import Sql Scripts"
for i in {1..50};
do
    
    if [ $? -eq 0 ]
    then
        for entry in "SqlToImport"/*
        do
            /opt/mssql-tools/bin/sqlcmd -S localhost,11433 -U sa -P DingoChat5% -d master -i $entry
            echo "Imported: "$entry
        done
        echo "Finished Importing sql scripts"
        break
    else
        echo "Waiting on MSSQL to start before importing sqls"
        sleep 1
    fi
done