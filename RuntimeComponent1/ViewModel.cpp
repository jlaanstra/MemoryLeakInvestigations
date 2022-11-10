#include "pch.h"
#include "ViewModel.h"
#include "ViewModel.g.cpp"

namespace winrt::RuntimeComponent1::implementation
{
    hstring ViewModel::Value()
    {
        return m_value;
    }

    void ViewModel::Value(hstring const& value)
    {
        m_value = value;
    }
}
