﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {
	public void Unpause () {
		MasterStateMachine.Instance.unpause();
	}

    public void ResetLevel() {
        Unpause();
        MasterStateMachine.Instance.ResetLevel();
    }

    public void GotoMainMenu () {
        MasterStateMachine.Instance.unpause();
        MasterStateMachine.Instance.setState(new MainMenuState());
    }
}
