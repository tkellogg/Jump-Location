
if (Test-Path Function:\TabExpansion) {
	Rename-Item Function:\TabExpansion PreJumpTabExpansion
}

function global:TabExpansion($line, $lastWord) {
	switch -regex ($line) {
		"^(Jump-Location|j|xj) .*" {
			[Jump.Location.JumpLocationCommand]::GetTabExpansion($line, $lastWord)
		}
		default {
			if (Test-Path Function:\PreJumpTabExpansion) {
				PreJumpTabExpansion $line $lastWord
			}
		}
	}
}
