package net.steampp.app.ui.fragments

import net.steampp.app.ui.viewmodels.GameListViewModel
import android.view.LayoutInflater
import android.view.ViewGroup
import android.os.Bundle
import android.view.View
import androidx.lifecycle.ViewModelProvider
import android.widget.TextView
import androidx.fragment.app.Fragment
import net.steampp.app.ui.databinding.FragmentGameListBinding

class ASFPlusFragment : Fragment() {
    private var viewModel: GameListViewModel? = null
    private var binding: FragmentGameListBinding? = null
    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?, savedInstanceState: Bundle?
    ): View? {
        viewModel = ViewModelProvider(this).get(GameListViewModel::class.java)
        binding = FragmentGameListBinding.inflate(inflater, container, false)
        val root: View = binding!!.root
        val textView: TextView = binding!!.textView
        viewModel!!.text.observe(viewLifecycleOwner, { s -> textView.text = s })
        return root
    }

    override fun onDestroyView() {
        super.onDestroyView()
        binding = null
    }
}
