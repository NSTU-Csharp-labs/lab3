using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls;

namespace lab3.Tools;

public class BitmapAssetValueConverter : IResourceDictionary
{
    public bool TryGetResource(object key, out object? value)
    {
        throw new NotImplementedException();
    }

    public bool HasResources { get; }
    public void AddOwner(IResourceHost owner)
    {
        throw new NotImplementedException();
    }

    public void RemoveOwner(IResourceHost owner)
    {
        throw new NotImplementedException();
    }

    public IResourceHost? Owner { get; }
    public event EventHandler? OwnerChanged;
    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public int Count { get; }
    public bool IsReadOnly { get; }
    public void Add(object key, object? value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(object key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(object key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(object key, out object? value)
    {
        throw new NotImplementedException();
    }

    public object? this[object key]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public ICollection<object> Keys { get; }
    public ICollection<object?> Values { get; }
    public IList<IResourceProvider> MergedDictionaries { get; }
}