using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using System;

public class ServiceBase : IDisposable {

    Service.Events.IEventsService _eventService;
    DisposableManager _dManager;
    CompositeDisposable disposables = new CompositeDisposable();

    [Inject]
    void Initialize(
        [Inject] Service.Events.IEventsService eventService,
        [Inject] DisposableManager dManager
        ) {
        _eventService = eventService;
        _dManager = dManager;

        _dManager.Add(this);

        AfterBind();
    }

    protected virtual void AfterBind() {}

    protected void Publish(object evt) {
        _eventService.Publish(evt);
    }

    protected IObservable<TEvent> OnEvent<TEvent>() {
        return _eventService.OnEvent<TEvent>();
    }

    public void AddDisposable(IDisposable disposable) {
        this.disposables.Add(disposable);
    }

    public void Dispose() {
        OnDispose();
    }

    protected virtual void OnDispose() { }
}
