
if (Test-Path Function:\TabExpansion) {
	Rename-Item Function:\TabExpansion PreJumpTabExpansion
}

function global:TabExpansion($line, $lastWord) {
	switch -regex ($line) {
		"^(Set-JumpLocation|j|xj|pushJ) .*" {
			[Jump.Location.SetJumpLocationCommand]::GetTabExpansion($line, $lastWord)
		}
		default {
			if (Test-Path Function:\PreJumpTabExpansion) {
				PreJumpTabExpansion $line $lastWord
			}
		}
	}
}
