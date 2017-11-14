<?php
$pipe_data_file = "pipe_data.json";

if(isset($_GET["close_pipe"]) && $_GET["close_pipe"] == "close_pipe_0decf8c3-4016-4cf7-a8db-40658b720da8"){
	//deleting the pipe data file
	unlink($pipe_data_file);
	echo($_GET["close_pipe"]);
}
else if(isset($_GET["pull_request"]) && $_GET["pull_request"] == "true"){
	if(file_exists($pipe_data_file)){
		echo file_get_contents($pipe_data_file);
	}
	else{
		echo "pull_failed_9f8f31d8-ffc8-4d8d-93b3-7f5f2cffcd47";
	}
}
else if(isset($_POST["PIPE_DATA"])){
	$fp = fopen($pipe_data_file, 'w');
    fwrite($fp, $_POST["PIPE_DATA"]);
    fclose($fp);
	
	echo "data_received_eea220ce-73d9-46c9-aa4e-664c8c47510a";
}else{
    echo("Invalid input");
}

?>