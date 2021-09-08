package net.steampp.app.ui.fragments

import net.steampp.app.ui.viewmodels.MyViewModel
import android.view.LayoutInflater
import android.view.ViewGroup
import android.os.Bundle
import android.view.View
import androidx.fragment.app.Fragment
import androidx.lifecycle.ViewModelProvider
import net.steampp.app.ui.databinding.FragmentMyBinding

class MyFragment : Fragment() {
    private var viewModel: MyViewModel? = null
    private var binding: FragmentMyBinding? = null
    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?, savedInstanceState: Bundle?
    ): View? {
        viewModel = ViewModelProvider(this).get(MyViewModel::class.java)
        binding =
            FragmentMyBinding.inflate(inflater, container, false)

//        final TextView textView = binding.textView;
//        viewModel.getText().observe(getViewLifecycleOwner(), new Observer<String>() {
//            @Override
//            public void onChanged(@Nullable String s) {
//                textView.setText(s);
//            }
//        });
        return binding!!.root
    }

    override fun onDestroyView() {
        super.onDestroyView()
        binding = null
    }
}
