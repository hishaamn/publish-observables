function RevisionsMatch($itemToCheck){
    if($(Test-Path "web:$($itemToCheck.Paths.Path)")){
        $webItem = Get-Item "web:$($itemToCheck.Paths.Path)" -lang $itemToCheck.Language
        if($webItem -ne $null)
        {
            if($webItem.Fields["__Revision"].Value -eq $itemToCheck.Fields["__Revision"].Value){
                $true
            }
        }            
    }
    $false
}

function ValidatePublishingRestrictions($itemToCheck){
    if($itemToCheck.'__Never publish' -eq 1){
        Write-Host "Skipping item [$($itemToCheck.ID)] because of publishing restrictions"
    }else{
        $itemToCheck
    }
}

$items = Get-ChildItem -Path "master:$path" -Language * -recurse;

[System.Collections.ArrayList]$itemList = @()

foreach($item in $items){
	$isSame = RevisionsMatch $item

	if($isSame -eq $False){
		$result = ValidatePublishingRestrictions $item
		
		if($result -ne $null){
			$itemList.Add($result)
		}
	}
}

return $itemList