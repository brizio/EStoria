﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EStoria.Interfaces;
using EStoria.ValueObjects;

namespace EStoria
{
	public abstract class CommandHandler : BaseDisposable, ICommandHandler
	{
		readonly Subject<DomainEvent> _eventsSubject;
		readonly Subject<CommandFailure> _failures;
		protected IModelLoader Loader { get; private set; } 

		protected CommandHandler(IModelLoader loader)
		{
			Guard.NotNull(() => loader);

			Loader = loader;
			_eventsSubject = new Subject<DomainEvent>();
			_failures = new Subject<CommandFailure>();
		}

		public void OnNext(ICommand command)
		{
			Guard.NotNull(() => command);

			var events = (IEnumerable<DomainEvent>)((dynamic)this).Apply((dynamic)command);
			events.ToObservable().Subscribe(_eventsSubject.OnNext);
		}

		public void OnError(Exception error)
		{
			_eventsSubject.OnError(error);
		}

		public void OnCompleted()
		{
			_eventsSubject.OnCompleted();
		}

		public IDisposable Subscribe(IObserver<DomainEvent> observer)
		{
			return _eventsSubject.Subscribe(observer);
		}

		public IDisposable Subscribe(IObserver<CommandFailure> observer)
		{
			return _failures.Subscribe(observer);
		}

		public virtual IEnumerable<DomainEvent> Apply(ICommand command)
		{
			yield break;
		}

		protected IEnumerable<DomainEvent> FailCommand(ICommand command, string reason)
		{
			_failures.OnNext(new CommandFailure(command, reason));
			yield break;
		}

		protected override void Dispose(bool disposing)
		{
			if(!Disposed && disposing)
			{
				_failures.OnCompleted();
				_eventsSubject.OnCompleted();
				_failures.Dispose();
				_eventsSubject.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}