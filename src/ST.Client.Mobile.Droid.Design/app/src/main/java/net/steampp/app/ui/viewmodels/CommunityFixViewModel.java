package net.steampp.app.ui.viewmodels;

import android.app.Application;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;

import net.steampp.app.design.R;
import net.steampp.app.ui.MainApplication;

import java.util.Arrays;

public class CommunityFixViewModel extends ViewModel {

    private MutableLiveData<String> mText;

    public CommunityFixViewModel() {
        mText = new MutableLiveData<>();
        mText.setValue(Arrays.stream(this.getClass().getName().split("\\.")).reduce((first, second) -> second).orElse("").replace("ViewModel","") + "\n" + MainApplication.getContext().getString(R.string.under_construction));
    }

    public LiveData<String> getText() {
        return mText;
    }
}
