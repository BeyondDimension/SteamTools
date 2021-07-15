package net.steampp.app.ui.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.Observer;
import androidx.lifecycle.ViewModelProvider;

import net.steampp.app.design.R;
import net.steampp.app.ui.viewmodels.LocalAuthViewModel;

public class LocalAuthFragment extends Fragment {

    private LocalAuthViewModel viewModel;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        viewModel =
                new ViewModelProvider(this).get(LocalAuthViewModel.class);

        View root = inflater.inflate(R.layout.fragment_local_auth_test, container, false);

        final TextView textView = root.findViewById(R.id.textView);
        viewModel.getText().observe(getViewLifecycleOwner(), new Observer<String>() {
            @Override
            public void onChanged(@Nullable String s) {
                textView.setText(s);
            }
        });
        return root;
    }
}
