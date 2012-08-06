
if (Test-Path Function:\TabExpansion) {
	Rename-Item Function:\TabExpansion PreJumpTabExpansion
}

function global:TabExpansion($line, $lastWord) {
	switch -regex ($line) {
		"^(Jump-Location|j) .*" {
			[Jump.Location.JumpLocationCommand]::GetTabExpansion($lastWord)
		}
		default {
			if (Test-Path Function:\PreJumpTabExpansion) {
				PreJumpTabExpansion $line $lastWord
			}
		}
	}
}
