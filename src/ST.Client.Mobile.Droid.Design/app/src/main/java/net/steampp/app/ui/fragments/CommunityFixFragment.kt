package net.steampp.app.ui.fragments

import net.steampp.app.ui.viewmodels.CommunityFixViewModel
import android.view.LayoutInflater
import android.view.ViewGroup
import android.os.Bundle
import android.view.View
import androidx.lifecycle.ViewModelProvider
import android.widget.TextView
import androidx.fragment.app.Fragment
import net.steampp.app.ui.databinding.FragmentCommunityFixBinding

class CommunityFixFragment : Fragment() {
    private var viewModel: CommunityFixViewModel? = null
    private var binding: FragmentCommunityFixBinding? = null
    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?, savedInstanceState: Bundle?
    ): View? {
        viewModel = ViewModelProvider(this).get(CommunityFixViewModel::class.java)
        binding = FragmentCommunityFixBinding.inflate(inflater, container, false)
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
