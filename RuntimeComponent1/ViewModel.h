#pragma once

#include "ViewModel.g.h"

namespace winrt::RuntimeComponent1::implementation
{
    struct ViewModel : ViewModelT<ViewModel>
    {
        ViewModel() = default;

        hstring Value();
        void Value(hstring const& value);

        Windows::Foundation::IInspectable Items() { return m_items; }
        void Items(Windows::Foundation::IInspectable const& value) { m_items = value; }

        event_token PropertyChanged(Microsoft::UI::Xaml::Data::PropertyChangedEventHandler const& handler)
        {
            return m_propertyChangedEvent.add(handler);
        }

        void PropertyChanged(event_token const& token) noexcept
        {
            m_propertyChangedEvent.remove(token);
        }

        void RaisePropertyChanged(hstring const& propertyName)
        {
            m_propertyChangedEvent(*this, Microsoft::UI::Xaml::Data::PropertyChangedEventArgs(propertyName));
        }

    private:
        event<Microsoft::UI::Xaml::Data::PropertyChangedEventHandler> m_propertyChangedEvent;

    private:
        hstring m_value;
        Windows::Foundation::IInspectable m_items;
    };
}

namespace winrt::RuntimeComponent1::factory_implementation
{
    struct ViewModel : ViewModelT<ViewModel, implementation::ViewModel>
    {
    };
}
